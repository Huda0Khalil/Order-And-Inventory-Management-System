using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Domain.Entities
{
    public class Order: IEntity, IMustHaveTenant
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public Customer? Customer { get; set; }
        public int? CustomerId { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal TotalAmount { get; set; }

        [Required]
        public string TenantId { get; set; }
    }
}
