using OAIM.Infrastructure.Data;

namespace OAIM.API.Extension
{
    public static class MigrationManager
    {
        public static async Task ApplyMigrationsAsync(
        this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var tenantSettings =
            scope.ServiceProvider.GetRequiredService<TenantSettings>();

            var tenantServices =
                scope.ServiceProvider.GetRequiredService<ITenantServices>();

            foreach (var tenant in tenantSettings.Tenants)
            {
                var connectionString =
                    tenant.ConnectionString
                    ?? tenantSettings.Default.ConnectionString;

                var optionsBuilder =
                    new DbContextOptionsBuilder<ApplicationDbContext>();

                optionsBuilder.UseSqlServer(connectionString);

                using var context =
                    new ApplicationDbContext(optionsBuilder.Options, tenantServices);
                await context.Database.MigrateAsync();
            }
        }
    }
}
