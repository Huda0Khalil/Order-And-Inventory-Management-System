using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OAIM.Application.Identity
{
    public interface IIdentityService
    {
        Task<string> CreateUserAsync(string email, string password, string tenantId, Guid domainUserId);
        Task<bool> CheckPasswordAsync(string email, string password, string tenantId);
    }
}
