using OAIM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.DTO
{
    public class ProductDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }

        public int SupplierId { get; set; }
        [Required]
        public string TenantId { get; set; }
    }
}
