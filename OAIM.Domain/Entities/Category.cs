using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Domain.Entities
{
    public class Category : IEntity, IMustHaveTenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        public string TenantId { get; set; }
    }
}