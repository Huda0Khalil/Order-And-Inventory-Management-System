using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OAIM.Application.DTO;
using OAIM.Domain.Interfaces;
using OAIM.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<User,Guid> _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(UserManager<ApplicationUser> userManager, IRepository<User, Guid> userRepository,ILogger<UserService> logger, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task DeleteUserAsync(Guid id)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {

                var identityUser = await _userManager.Users
                      .FirstOrDefaultAsync(u => u.DomainUserId == id);

                if (identityUser == null)
                    throw new Exception("User not found");


                await _userManager.DeleteAsync(identityUser);

                var domainUser = await _userRepository.GetAll().FirstOrDefaultAsync(i => i.Id == id);
                if (domainUser != null)
                    _userRepository.Delete(domainUser);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("User deleted | UserId: {UserId}", id);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error deleting user | UserId: {UserId}", id);
                throw;

            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var identityUsers = _userManager.Users.ToList();

            var result = new List<UserDto>();
            foreach (var identityUser in identityUsers)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                result.Add(new UserDto
                {
                    Id = identityUser.DomainUserId,
                    Email = identityUser.Email,
                    UserName = identityUser.UserName,
                    PhoneNumber = identityUser.PhoneNumber,
                    Role = roles.FirstOrDefault(),
                    TenantId = identityUser.TenantId
                });
            }

            return result;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            //must do : add order that created by this user to the user dto ( we will use userRepository)
            var identityUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.DomainUserId == id);

            if (identityUser == null) return null;

            var roles = await _userManager.GetRolesAsync(identityUser);

            return new UserDto
            {// add orders to the user dto
                Id = identityUser.DomainUserId,
                Email = identityUser.Email,
                UserName = identityUser.UserName,
                PhoneNumber = identityUser.PhoneNumber,
                Role = roles.FirstOrDefault(),
                TenantId = identityUser.TenantId
            };
        }

        public async Task UpdateUserRoleAsync(Guid id, UpdateRoleDto dto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Role))
                    throw new ArgumentException("Role is required");

                var identityUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DomainUserId == id);

                if (identityUser == null)
                    throw new Exception("User not found");

                var currentRoles = await _userManager.GetRolesAsync(identityUser);

                if (currentRoles.Any() && !currentRoles.Contains(dto.Role))
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                    if (!removeResult.Succeeded)
                        throw new Exception(string.Join(", ", removeResult.Errors.Select(e => e.Description)));

                    var addResult = await _userManager.AddToRoleAsync(identityUser, dto.Role);
                    if (!addResult.Succeeded)
                        throw new Exception(string.Join(", ", addResult.Errors.Select(e => e.Description)));

                    _logger.LogInformation(
                        "Role updated | UserId: {UserId} | NewRole: {Role}",
                        id,
                        dto.Role
                    );
                }
                identityUser.UserName = dto.UserName;
                identityUser.Email = dto.Email;
                identityUser.PhoneNumber = dto.PhoneNumber;
                
                var updateResult = await _userManager.UpdateAsync(identityUser);

                if (!updateResult.Succeeded)
                    throw new Exception(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    throw new Exception("Domain user not found");

                user.Email = dto.Email;
                user.UserName = dto.UserName;
                user.PhoneNumber = dto.PhoneNumber;
                user.TenantId = identityUser.TenantId;

                await _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error updating user role | UserId: {UserId} | NewRole: {Role}",
                    id,
                    dto.Role
                );

                throw;
            }
        }
    }
}
