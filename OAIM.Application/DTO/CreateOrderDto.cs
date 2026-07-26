using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.DTO
{
    public class CreateOrderDto
    {
        public Guid? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public Guid? UpdatedById { get; set; }
        public string? UpdatedByName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? CustomerId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; }
        public CustomerType CustomerType { get; set; } 

    }
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
