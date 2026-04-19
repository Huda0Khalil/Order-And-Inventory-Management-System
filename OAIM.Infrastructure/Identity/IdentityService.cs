using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OAIM.Application.Identity;
using OAIM.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> CreateUserAsync(string email, string password, string tenantId, Guid domainUserId)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            TenantId = tenantId,
            DomainUserId = domainUserId
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            throw new Exception("User creation failed");

        return user.Id;
    }

    public async Task<bool> CheckPasswordAsync(string email, string password, string tenantId)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || user.TenantId != tenantId)
            return false;

        var result = await _userManager.CheckPasswordAsync(user, password);

        return result;
    }
}
