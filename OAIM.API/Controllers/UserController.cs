using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAIM.Application.DTO;
using System.Security.Claims;

namespace OAIM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromHeader(Name = "tenant")] string tenant)
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromHeader(Name = "tenant")] string tenant,Guid id)
        {
            var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var requesterRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Employee can only view their own profile
            if (requesterRole == "Employee" && requesterId != id.ToString())
            {
               return Forbid();
            }

            var user = await _userService.GetUserByIdAsync(Guid.Parse(User.FindFirst("domainUserId")?.Value));
            if (user == null) return NotFound("User not found");

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromHeader(Name ="tenant")]string tenant,Guid id)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogWarning("DeleteUser called | UserId: {UserId} | by AdminId: {AdminId}",
                id, adminId);

            await _userService.DeleteUserAsync(id);
            return Ok(new { Message = "User deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole([FromHeader(Name ="tenant")]string tenant,Guid id, [FromBody] UpdateRoleDto dto)
        {
            await _userService.UpdateUserRoleAsync(id, dto);
            return Ok(new { Message = "Role updated successfully" });
        }
    }

    }
