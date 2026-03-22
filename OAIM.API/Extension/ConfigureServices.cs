using Microsoft.Extensions.Options;
using OAIM.Infrastructure.Data;

namespace OAIM.API.Extension
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddTenancy(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TenantSettings>(
        configuration.GetSection("TenantSettings"));

            services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<TenantSettings>>().Value);

            services.AddScoped<ITenantServices, TenantServices>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var tenantService = sp.GetRequiredService<ITenantServices>();
                var tenantSettings = sp.GetRequiredService<TenantSettings>();

                var connectionString =
                    tenantService.GetTenantConnectionString()
                    ?? tenantSettings.Default.ConnectionString;

                options.UseSqlServer(connectionString);
            });
            return services;
            ////var tenantSettings = services.Configure<TenantSettings>(configuration.GetSection("TenantSettings"));
            //var tenantSettings = configuration.GetSection("TenantSettings").Get<TenantSettings>();

            //services.AddScoped<ITenantServices, TenantServices>();
            ////services.AddDbContextFactory<ApplicationDbContext>(
            ////    opt => opt.UseSqlServer(configuration.GetConnectionString("TenantSettings:Default:ConnectionString"))); 
            //services.AddDbContext<ApplicationDbContext>((sp, options) =>
            //{
            //    var tenantService = sp.GetRequiredService<ITenantServices>();
            //    var connectionString =
            //        tenantService.GetTenantConnectionString()
            //        ?? tenantSettings.Default.ConnectionString;
            //    options.UseSqlServer(connectionString);
            //});

            //return services;
        }
    }
            //foreach (var tenant in options.Tenants)
            //{
            //    var connectionString = tenant.ConnectionString ?? options.Default.ConnectionString;

            //    using var scope = services.BuildServiceProvider().CreateScope();
            //    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //    dbContext.Database.SetConnectionString(connectionString);

            //    if (dbContext.Database.GetPendingMigrations().Any())
            //    {
            //        dbContext.Database.Migrate();
            //    }
            //}
            // Add API specific services here
        
    
}
