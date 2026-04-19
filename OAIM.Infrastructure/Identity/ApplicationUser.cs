using Microsoft.AspNetCore.Identity;

namespace OAIM.Infrastructure.Identity
{
    public class ApplicationUser: IdentityUser
    {
        public string TenantId { get; set; }
        // Link to Domain User
        public Guid DomainUserId { get; set; }
    }
}
