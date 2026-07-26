using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.DTO
{
    public class DashboardDto
    {
        public int TotalOrdersToday { get; set; }

        public decimal RevenueToday { get; set; }

        public int TotalProducts { get; set; }

        public int ActiveCustomers { get; set; }

        public List<RecentOrderDto> RecentOrders { get; set; }

        public List<LowStockProductDto> LowStockProducts { get; set; }
    }
}
