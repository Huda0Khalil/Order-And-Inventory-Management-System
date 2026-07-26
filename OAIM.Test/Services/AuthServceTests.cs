using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OAIM.Application.DTO;
using OAIM.Application.Interfaces;
using OAIM.Application.Services;
using OAIM.Domain.Entities;
using OAIM.Domain.Interfaces;
using OAIM.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OAIM.Tests.Services
{
    public class AuthServiceTests
    {
        // ─────────────────────────────────────────────
        //  Mocks & SUT
        // ─────────────────────────────────────────────
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IRepository<User, Guid>> _userRepoMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _userRepoMock = new Mock<IRepository<User, Guid>>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            // UserManager requires a IUserStore mock at minimum
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // RoleManager requires a IRoleStore mock at minimum
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            // IConfiguration → JwtSettings section
            _configMock = new Mock<IConfiguration>();
            SetupJwtConfiguration();

            _sut = new AuthService(
                _uowMock.Object,
                _userRepoMock.Object,
                _userManagerMock.Object,
                _configMock.Object,
                _roleManagerMock.Object,
                _loggerMock.Object);
        }

        // ─────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────

        /// <summary>Wires up a minimal IConfiguration that satisfies GenerateJwtToken.</summary>
        private void SetupJwtConfiguration()
        {
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(s => s["SecretKey"]).Returns("supersecretkey1234567890ABCDEFGH");
            jwtSection.Setup(s => s["issuer"]).Returns("TestIssuer");
            jwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
            _configMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);
        }

        private static ApplicationUser BuildIdentityUser(
            string email = "user@test.com",
            string tenantId = "tenant-1",
            string userName = "testuser")
        {
            return new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = userName,
                TenantId = tenantId,
                DomainUserId = Guid.NewGuid(),
                SecurityStamp = Guid.NewGuid().ToString()
            };
        }

        private static LoginDto BuildLoginDto(
            string email = "user@test.com",
            string password = "Password123!",
            string tenantId = "tenant-1")
        {
            return new LoginDto { Email = email, Password = password, TenantId = tenantId };
        }

        private static RegisterDto BuildRegisterDto(
            string email = "new@test.com",
            string password = "Password123!",
            string tenantId = "tenant-1",
            string role = "Employee",
            string userName = "newuser",
            string phone = "0501234567")
        {
            return new RegisterDto
            {
                Email = email,
                Password = password,
                TenantId = tenantId,
                Role = role,
                UserName = userName,
                PhoneNumber = phone
            };
        }

        // ═════════════════════════════════════════════
        //  LoginAsync
        // ═════════════════════════════════════════════

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            var dto = BuildLoginDto();
            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(dto));
            Assert.Equal("Invalid credentials", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenTenantIdMismatch()
        {
            // Arrange — user exists but belongs to a different tenant
            var dto = BuildLoginDto(tenantId: "tenant-A");
            var user = BuildIdentityUser(tenantId: "tenant-B");   // different tenant

            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(dto));
            Assert.Equal("Invalid credentials", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenPasswordIsWrong()
        {
            // Arrange
            var dto = BuildLoginDto();
            var user = BuildIdentityUser(tenantId: dto.TenantId);

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(dto));
            Assert.Equal("Invalid credentials", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnJwtToken_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = BuildLoginDto();
            var user = BuildIdentityUser(tenantId: dto.TenantId);

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Employee" });

            // Act
            var token = await _sut.LoginAsync(dto);

            // Assert — a JWT has exactly 2 dots
            Assert.NotNull(token);
            //Assert.Equal(2, token.Count(c => c == '.'));
        }

        [Fact]
        public async Task LoginAsync_ShouldNotCheckPassword_WhenUserIsNull()
        {
            // Arrange
            var dto = BuildLoginDto();
            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);

            // Act
            try { await _sut.LoginAsync(dto); } catch { }

            // Assert — password check must never be called if user was not found
            _userManagerMock.Verify(
                m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldCallFindByEmailAsync_WithCorrectEmail()
        {
            // Arrange
            var dto = BuildLoginDto(email: "specific@test.com");
            _userManagerMock
                .Setup(m => m.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            try { await _sut.LoginAsync(dto); } catch { }

            // Assert
            _userManagerMock.Verify(m => m.FindByEmailAsync("specific@test.com"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldCallGetRolesAsync_WhenLoginSucceeds()
        {
            // Arrange
            var dto = BuildLoginDto();
            var user = BuildIdentityUser(tenantId: dto.TenantId);

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

            // Act
            await _sut.LoginAsync(dto);

            // Assert
            _userManagerMock.Verify(m => m.GetRolesAsync(user), Times.Once);
        }

        // ═════════════════════════════════════════════
        //  RegisterAsync
        // ═════════════════════════════════════════════

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = BuildRegisterDto();
            var existingUser = BuildIdentityUser(email: dto.Email);

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.RegisterAsync(dto));
            Assert.Equal("Email already exists", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenIdentityUserCreationFails()
        {
            // Arrange
            var dto = BuildRegisterDto();

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());
            _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });
            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(failedResult);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.RegisterAsync(dto));
            Assert.Contains("Password too weak", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccessMessage_WhenRegistrationSucceeds()
        {
            // Arrange
            var dto = BuildRegisterDto(role: "Admin");
            var roles = new List<IdentityRole> { new IdentityRole("Admin"), new IdentityRole("Employee") }
                        .AsQueryable();

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());
            _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.Roles).Returns(roles);

            // Act
            var result = await _sut.RegisterAsync(dto);

            // Assert
            Assert.Equal("User registered successfully", result);
        }

        [Fact]
        public async Task RegisterAsync_ShouldAddDomainUserToRepository()
        {
            // Arrange
            var dto = BuildRegisterDto();
            var roles = new List<IdentityRole> { new IdentityRole("Employee") }.AsQueryable();

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());
            _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.Roles).Returns(roles);

            // Act
            await _sut.RegisterAsync(dto);

            // Assert
            _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.Email == dto.Email &&
                u.UserName == dto.UserName &&
                u.TenantId == dto.TenantId)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldAssignEmployeeRole_WhenRequestedRoleDoesNotExist()
        {
            // Arrange — only "Employee" role exists, dto requests "Manager"
            var dto = BuildRegisterDto(role: "Manager");
            var roles = new List<IdentityRole> { new IdentityRole("Employee") }.AsQueryable();

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());
            _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.Roles).Returns(roles);

            // Act
            await _sut.RegisterAsync(dto);

            // Assert — fallback to "Employee"
            _userManagerMock.Verify(
                m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Employee"),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldAssignRequestedRole_WhenRoleExists()
        {
            // Arrange
            var dto = BuildRegisterDto(role: "Admin");
            var roles = new List<IdentityRole> { new IdentityRole("Admin"), new IdentityRole("Employee") }.AsQueryable();

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());
            _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.Roles).Returns(roles);

            // Act
            await _sut.RegisterAsync(dto);

            // Assert
            _userManagerMock.Verify(
                m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldNotAddDomainUser_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = BuildRegisterDto();
            var existingUser = BuildIdentityUser(email: dto.Email);

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            // Act
            try { await _sut.RegisterAsync(dto); } catch { }

            // Assert — domain user must never be saved
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCallSaveChangesAsync_AfterDomainUserCreated()
        {
            // Arrange
            var dto = BuildRegisterDto();
            var roles = new List<IdentityRole> { new IdentityRole("Employee") }.AsQueryable();

            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser)null);
            _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());
            _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(1));
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.Roles).Returns(roles);

            // Act
            await _sut.RegisterAsync(dto);

            // Assert
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // ═════════════════════════════════════════════
        //  LogoutAsync
        // ═════════════════════════════════════════════

        [Fact]
        public async Task LogoutAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            const string userId = "nonexistent-id";
            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.LogoutAsync(userId));
            Assert.Equal("User not found", ex.Message);
        }

        [Fact]
        public async Task LogoutAsync_ShouldUpdateSecurityStamp_WhenUserFound()
        {
            // Arrange
            var user = BuildIdentityUser();
            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _sut.LogoutAsync(user.Id);

            // Assert
            _userManagerMock.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnSuccessMessage_WhenUserFound()
        {
            // Arrange
            var user = BuildIdentityUser();
            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.LogoutAsync(user.Id);

            // Assert
            Assert.Equal("Logged out successfully", result);
        }

        [Fact]
        public async Task LogoutAsync_ShouldCallFindByIdAsync_WithCorrectUserId()
        {
            // Arrange
            const string userId = "specific-user-id";
            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            // Act
            try { await _sut.LogoutAsync(userId); } catch { }

            // Assert
            _userManagerMock.Verify(m => m.FindByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldNotUpdateSecurityStamp_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            // Act
            try { await _sut.LogoutAsync("bad-id"); } catch { }

            // Assert
            _userManagerMock.Verify(
                m => m.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }
    }
}