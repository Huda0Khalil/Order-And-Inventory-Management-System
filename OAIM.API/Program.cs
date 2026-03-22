using Microsoft.OpenApi.Models;
using OAIM.API.Extension;
using OAIM.Domain.Interfaces;
using OAIM.Infrastructure.RepositoryImp;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTenancy(builder.Configuration);
builder.Services.Configure<TenantSettings>(builder.Configuration.GetSection("TenantSettings"));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(option =>
    option.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Oeder And Inventory Management System",
        Version = "v1",
        Description = "API for managing a OAIM System",
    })
); builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IRepository<Product>, Repository<Product>>();
builder.Services.AddTransient<IRepository<Category>, Repository<Category>>();
builder.Services.AddTransient<IRepository<Supplier>, Repository<Supplier>>();
builder.Services.AddTransient<IRepository<Customer>, Repository<Customer>>();
builder.Services.AddTransient<IRepository<Order>, Repository<Order>>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
var app = builder.Build();
await app.ApplyMigrationsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
