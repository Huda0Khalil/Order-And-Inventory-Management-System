using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Domain.Entities
{
    public class Customer:IEntity<int>,IMustHaveTenant
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
      
        [Required]
        public string TenantId { get; set; }
    }
    
}
