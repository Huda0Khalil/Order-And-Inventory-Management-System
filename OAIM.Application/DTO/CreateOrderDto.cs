using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.DTO
{
    public class CreateOrderDto
    {
        public int? CustomerId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; }

    }
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
