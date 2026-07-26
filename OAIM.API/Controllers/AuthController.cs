using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using OAIM.Application.DTO;
using System.Linq;
using System.Security.Claims;

namespace OAIM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromHeader(Name = "tenant")] string tenant, [FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(dto);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                };

                Response.Cookies.Append("auth_token", result.Token, cookieOptions);
                return Ok(new
                {
                    message = "Login successful",
                    user = new
                    {
                        userName = result.UserName,
                        userId = result.UserId,
                        email = result.Email,
                        role = result.Role
                    }
                    
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromHeader(Name = "tenant")] string tenant,[FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterAsync(dto);

                return Ok(new
                {
                    message = "User registered successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromHeader(Name = "tenant")] string tenant, [FromBody]RegisterDto request)
        {
            request.Role = "Admin";
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader(Name = "tenant")] string tenant)
        {
            Console.WriteLine(User);
            // Get user ID from JWT claims
            var userId = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token");

            var result = await _authService.LogoutAsync(userId);
            Response.Cookies.Append("auth_token", "", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            }); return Ok(new { Message = result });
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                User.Identity!.Name,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

    }
}
