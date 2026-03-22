using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.Settings
{
    public class TenantSettings
    {
        public TenantConfiguration Default { get; set; } = default!;
        public List<Tenant> Tenants { get; set; } = new();
    }
}
