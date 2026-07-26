using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.DTO
{
    public class RecentOrderDto
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; }

        public string CustomerName { get; set; }

        public string Type { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
