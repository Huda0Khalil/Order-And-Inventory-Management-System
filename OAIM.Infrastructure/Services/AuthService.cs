using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OAIM.Application.DTO;
using OAIM.Domain.Entities;
using OAIM.Domain.Interfaces;
using OAIM.Infrastructure.Identity;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace OAIM.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger _logger;



        public AuthService(IUnitOfWork unitOfWork, IRepository<User, Guid> userRepository, UserManager<ApplicationUser> userManager,
            IConfiguration configuration, RoleManager<IdentityRole> roleManager, ILogger<AuthService> logger
)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _logger = logger;
        } 

        public async Task<string> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || user.TenantId != dto.TenantId)
            {
                _logger.LogWarning("Login failed for email: {Email}. User not found or tenant mismatch.", dto.Email);
                throw new Exception("Invalid credentials");
            }
                

            var result = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!result)
            {
                _logger.LogWarning("Login failed — wrong password for email: {Email}", dto.Email);
                throw new Exception("Invalid credentials");

            }
            _logger.LogInformation("Login successful for email: {Email} | TenantId: {TenantId}",
            dto.Email, user.TenantId);
            return await GenerateJwtToken(user);
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation($"Register attempt for email: { dto.Email} | TenantId: {dto.TenantId}");
            var existingIdentity = await _userManager.FindByEmailAsync(dto.Email);
            if (existingIdentity != null)
            {
                _logger.LogWarning($"Register failed — email already exists: {dto.Email}");
                throw new Exception("Email already exists");

            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var domainUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email,
                    UserName = dto.UserName,
                    PhoneNumber = dto.PhoneNumber,
                    TenantId = dto.TenantId
                };

                await _userRepository.AddAsync(domainUser);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"DomainUser created successfully | UserId: {domainUser.Id} | Email: {dto.Email}");
                var identityUser = new ApplicationUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    TenantId = dto.TenantId,
                    DomainUserId = domainUser.Id,
                    PhoneNumber = dto.PhoneNumber                                      
                };

                var result = await _userManager.CreateAsync(identityUser, dto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("IdentityUser creation failed for email: {dto.Email} | Errors: {Errors}",
                    dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                }

                var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                var roleToAssign = allRoles.Contains(dto.Role) ? dto.Role : "Employee";
                await _userManager.AddToRoleAsync(identityUser, roleToAssign);
                _logger.LogInformation($"IdentityUser created successfully | Email: {dto.Email} | Role: {dto.Role}",
                dto.Email, roleToAssign);
                await transaction.CommitAsync();

                return "User registered successfully";
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Registration transaction rolled back for email: {dto.Email}", dto.Email);
                throw;
            }
        }
        public async Task<string> LogoutAsync(string userId)
        {
            _logger.LogInformation($"Logout attempt for userId: {userId}");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                _logger.LogWarning($"Logout failed — user not found: {userId}", userId);
                throw new Exception("User not found");

            }

            // 2. Invalidate all existing tokens by updating security stamp
            // This forces all issued tokens to become invalid immediately
            await _userManager.UpdateSecurityStampAsync(identityUser);
            _logger.LogInformation($"Logout successful — security stamp updated for userId: {userId} | Email: {identityUser.Email}");

            return "Logged out successfully";
        }
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var claims = new List<Claim>
            {
                new Claim("tenantId", user.TenantId),
                new Claim("domainUserId", user.DomainUserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("AspNet.Identity.SecurityStamp", user.SecurityStamp)

            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
