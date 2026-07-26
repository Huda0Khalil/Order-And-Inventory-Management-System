using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Domain.Entities
{
    public class Order: IEntity<int>, IMustHaveTenant
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public Customer? Customer { get; set; }
        public int? CustomerId { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal TotalAmount { get; set; }

        [Required]
        public string TenantId { get; set; }
        public User CreatedBy { get; set; }
        public Guid CreatedById { get; set; }
        
        public User? UpdatedBy { get; set; }
        public Guid? UpdatedById { get; set; }
        public DateTime? UpdateDate { get; set; }
        public CustomerType CustomerType { get; set; } = CustomerType.
           Retail;

    }
    public enum CustomerType
    {
        Retail,
        Wholesale
    }
}
