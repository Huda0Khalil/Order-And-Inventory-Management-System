using OAIM.Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.IServices
{
    public interface ITenantServices
    {
        Tenant GetCurrentTenant();
        string GetTenantConnectionString();
        string GetTenantDatabaseProvider();

    }
}
