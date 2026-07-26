using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.DTO
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
        public CustomerType CustomerType { get; set; }

    }
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
