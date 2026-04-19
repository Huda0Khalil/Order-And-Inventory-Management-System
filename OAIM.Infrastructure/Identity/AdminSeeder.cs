using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Infrastructure.Identity
{
    public class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration config)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var userRepository = serviceProvider.GetRequiredService<IRepository<User,Guid>>();

            // Seed Roles
            string[] roleNames = { "Admin", "Employee" };
            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin
            var adminEmail = config["AdminSettings:Email"];
            var adminPassword = config["AdminSettings:Password"];
            var adminTenantId = config["AdminSettings:TenantId"];

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var domainUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = adminEmail,
                    UserName = "Admin",
                    PhoneNumber = "",
                    TenantId = adminTenantId
                };

                await userRepository.AddAsync(domainUser);
                await unitOfWork.SaveChangesAsync();

                var identityUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    TenantId = adminTenantId,
                    DomainUserId = domainUser.Id
                };

                var result = await userManager.CreateAsync(identityUser, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(identityUser, "Admin");
            }
        }
    }
}

