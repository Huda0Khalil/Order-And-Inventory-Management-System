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
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
