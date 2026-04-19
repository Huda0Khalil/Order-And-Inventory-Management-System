using System.ComponentModel.DataAnnotations;
namespace OAIM.Application.DTO
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string TenantId { get; set; }
    }
}
