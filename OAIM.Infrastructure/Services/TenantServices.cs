using OAIM.Application.IServices;
using OAIM.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace OAIM.Application.Services
{
    public class TenantServices : ITenantServices
    {
        private readonly TenantSettings _tenantSettings;
        private Tenant? _currentTenant;
        private HttpContext? httpContext;
        public TenantServices(IHttpContextAccessor contextAccessor,IOptions<TenantSettings> tenantSettings)
        {
            _tenantSettings = tenantSettings.Value;
            httpContext = contextAccessor.HttpContext;
            if (httpContext is not null) 
            {
                if(httpContext.Request.Headers.TryGetValue("tenant", out var tenantId))
                {
                    SetCurrentTenant(tenantId);    
                }
                else
                {
                    throw new Exception("No tenant provided!");
                }
            }
        }
        public Tenant GetCurrentTenant()
        {
            return _currentTenant!;
        }

        public string GetTenantConnectionString()
        {
            return _currentTenant?.ConnectionString ?? _tenantSettings.Default.ConnectionString;
        }

        public string GetTenantDatabaseProvider()
        {
            return  _tenantSettings.Default.DBProvider;
        }
        private void SetCurrentTenant(string tenantId)
        {
            _currentTenant = _tenantSettings.Tenants.FirstOrDefault(t => t.TId == tenantId);
            if(_currentTenant is null)
            {
                throw new Exception("Invalid tenant!");
            }
            if(string.IsNullOrEmpty(_currentTenant.ConnectionString))
            {
                _currentTenant.ConnectionString = _tenantSettings.Default.ConnectionString;
            }
        }
    }
}
