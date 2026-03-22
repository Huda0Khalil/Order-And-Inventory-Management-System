using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.Settings
{
   public class TenantConfiguration
    {
        public string DBProvider { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
    }
}
