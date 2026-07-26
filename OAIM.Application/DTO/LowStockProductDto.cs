using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.DTO
{
    public class LowStockProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string SKU { get; set; }

        public int Stock { get; set; }

        public int MinimumStock { get; set; }
    }
}
