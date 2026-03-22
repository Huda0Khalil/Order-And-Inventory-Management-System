using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.Settings
{
    public class Tenant
    {
        public string Name { get; set; } = null!;
        public string TId { get; set; } = null!;
        public string? ConnectionString { get; set; }
    }
}
