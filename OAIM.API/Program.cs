using Microsoft.OpenApi.Models;
using OAIM.API.Extension;
using OAIM.Infrastructure.RepositoryImp;
using System.Text.Json.Serialization;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OAIM.Application.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using OAIM.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTenancy(builder.Configuration);
builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection("TenantSettings"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettingsSection["Issuer"],
            ValidAudience = jwtSettingsSection["Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettingsSection["SecretKey"])),
            ClockSkew = TimeSpan.Zero
        };
        // Validate security stamp on every request
        option.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>();

                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    context.Fail("Unauthorized");
                    return;
                }

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    context.Fail("Unauthorized");
                    return;
                }

                // Check if security stamp in token matches DB
                var stampClaim = context.Principal?.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
                if (stampClaim != null && stampClaim != user.SecurityStamp)
                {
                    context.Fail("Token is no longer valid"); // logout invalidates this
                    return;
                }
            }
        };
    });

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(option =>
    option.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Oeder And Inventory Management System",
        Version = "v1",
        Description = "API for managing a OAIM System",
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Host.UseSerilog((context, confguration) => confguration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddTransient<IRepository<User,Guid>, Repository<User, Guid>>();
builder.Services.AddTransient<IRepository<Product,int>, Repository<Product,int>>();
builder.Services.AddTransient<IRepository<Category, int>, Repository<Category, int>>();
builder.Services.AddTransient<IRepository<Supplier, int>, Repository<Supplier, int>>();
builder.Services.AddTransient<IRepository<Customer, int>, Repository<Customer, int>>();
builder.Services.AddTransient<IRepository<Order,int>, Repository<Order,int>>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<OrderProfile>());
var app = builder.Build();
await app.ApplyMigrationsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
    //var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    //string[] roleNames = { "Admin", "Employee" };

    //foreach (var role in roleNames)
    //{
    //    var roleExist = await roleManager.RoleExistsAsync(role);
    //    if (!roleExist)
    //    {
    //        await roleManager.CreateAsync(new IdentityRole(role));
    //    }
    //    var adminEmail = builder.Configuration["AdminSettings:Email"];
    //    var adminPassword = builder.Configuration["AdminSettings:Password"];
    //    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    //    if (adminUser == null)
    //    {
    //        var newAdmin = new ApplicationUser
    //        {
    //            UserName = adminEmail,
    //            Email = adminEmail,
    //            NormalizedUserName = adminEmail.ToUpper(),
    //            Name = "System Admin",
    //            EmailConfirmed = true
    //        };

    //        var result = await userManager.CreateAsync(newAdmin, adminPassword);
    //        if (result.Succeeded)
    //            await userManager.AddToRoleAsync(newAdmin, "Admin");
    //    }

    //}
}
app.Run();
