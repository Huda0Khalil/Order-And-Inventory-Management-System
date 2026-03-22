using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using OAIM.Application.Settings;


namespace OAIM.Infrastructure.Data
{
    public class ApplicationDbContextFactory
                    : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "OAIM.API");
            // Load API appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Use Get<T> method directly without importing Microsoft.Extensions.Configuration.Binder
            var tenantSettings = configuration
           .GetSection("TenantSettings")
           .Get<TenantSettings>();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(
                tenantSettings!.Default.ConnectionString);

            // ❌ DO NOT inject ITenantServices at design time
            return new ApplicationDbContext(optionsBuilder.Options, null!);
        }
    }
  
}
