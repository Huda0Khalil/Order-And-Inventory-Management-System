using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Domain.Entities
{
    public class Product : IEntity, IMustHaveTenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
        public int StockQuantity { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }

        public Supplier Supplier { get; set; }
        public int SupplierId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        [Required]
        public string TenantId { get; set; }
    }
}
