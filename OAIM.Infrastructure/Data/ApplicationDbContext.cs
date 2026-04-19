
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OAIM.Domain.Interfaces;
using OAIM.Infrastructure.Identity;

namespace OAIM.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantServices? _tenantService;
        private readonly IConfiguration _configuration;
        public string? TenantId { get; set; }
        public string? connectionString { get; set; }

        private readonly ITenantServices? _tenantServices;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantServices? tenantServices)
            : base(options)
        {
            _tenantServices = tenantServices;
            if (_tenantServices != null)
            {
                var tenant = _tenantServices.GetCurrentTenant();
                TenantId = tenant?.TId;
                connectionString = tenant?.ConnectionString;
            }

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && _tenantServices != null)
            {
                optionsBuilder.UseSqlServer(_tenantServices.GetTenantConnectionString());
            }
        }
        //public ApplicationDbContext(DbContextOptions Options, 
        //    ITenantServices? tenantServices,
        //    IConfiguration? configuration 
        //    ) : base(Options)
        //{
        //    _tenantService = tenantServices;
        //    _configuration = configuration;
        //    //if(_tenantService is not null)
        //    //{
        //    //    var tenant = _tenantService.GetCurrentTenant();
        //    //    TenantId = tenant?.TId;
        //    //    connectionString = tenant?.ConnectionString;
        //    //}

        //}

        DbSet<Customer> Customers { get; set; }
        DbSet<Supplier> Suppliers { get; set; }
        DbSet<Order> Orders { get; set; }
        DbSet<OrderItem> OrderItems { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<User> UsersDomain { get; set; }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var x = ChangeTracker.Entries<IMustHaveTenant>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified).ToList();
            foreach (var entry in x)
            {
                entry.Entity.TenantId = TenantId;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>().Where(e => e.State == EntityState.Added))
            {
                entry.Entity.TenantId = TenantId;
            }
            return base.SaveChanges();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == TenantId);
            modelBuilder.Entity<User>().HasMany(u => u.Orders).WithOne(o => o.CreatedBy)
                .HasForeignKey(o => o.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.UpdatedBy)
                .WithMany()
                .HasForeignKey(o => o.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.TenantId == TenantId);
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => s.TenantId == TenantId);
            modelBuilder.Entity<Order>().HasMany(o => o.Items).WithOne(o => o.Order)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>().HasQueryFilter(o => o.TenantId == TenantId);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => oi.TenantId == TenantId);
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.TenantId == TenantId);
            modelBuilder.Entity<Product>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<Category>().HasQueryFilter(c => c.TenantId == TenantId);
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var tenant = _tenantService.GetTenantConnectionString();
        //    optionsBuilder.UseSqlServer(tenant);
        //    //if(!optionsBuilder.IsConfigured)
        //    //{
        //    //   //if(_tenantService ==null)
        //    //   // {
        //    //   //     var configuration = new ConfigurationBuilder()
        //    //   // .SetBasePath(Directory.GetCurrentDirectory())
        //    //   // .AddJsonFile("appsettings.json", optional: true)
        //    //   // .Build();
        //    //   //     var provider =
        //    //   //         configuration["TenantSettings:Default:DBProvider"];
        //    //   //     var defaultConnectionString =
        //    //   //         configuration["TenantSettings:Default:ConnectionString"];
        //    //   //     if (provider?.ToLower() == "mssql")
        //    //   //     {
        //    //   //         optionsBuilder.UseSqlServer(defaultConnectionString);
        //    //   //     }
        //    //   //     else if (!string.IsNullOrEmpty(connectionString))
        //    //   //     {
        //    //   //         // For runtime, use tenant connection string
        //    //   //         var dbProvider = _tenantService.GetTenantDatabaseProvider();
        //    //   //         if (dbProvider?.ToLower() == "mssql")
        //    //   //         {
        //    //   //             optionsBuilder.UseSqlServer(connectionString);
        //    //   //         }
        //    //   //     }
        //    //   // }
        //    //}

        //}

    }
}
