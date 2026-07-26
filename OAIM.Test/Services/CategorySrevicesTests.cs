using Moq;
using Xunit;
using OAIM.Application.Services;
using OAIM.Application.DTO;
using OAIM.Domain.Entities;
using OAIM.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using OAIM.Application.Interfaces;

namespace OAIM.Tests.Services
{
    public class CategoryServiceTests
    {
        // ─────────────────────────────────────────────
        //  Mocks & SUT
        // ─────────────────────────────────────────────
        private readonly Mock<IRepository<Category, int>> _repoMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _repoMock = new Mock<IRepository<Category, int>>();
            _uowMock = new Mock<IUnitOfWork>();
            _sut = new CategoryService(_repoMock.Object, _uowMock.Object);
        }

        // ─────────────────────────────────────────────
        //  Helper
        // ─────────────────────────────────────────────
        private static CategoryDto BuildDto(
            string name = "Electronics",
            string tenantId = "tenant-1")
        {
            return new CategoryDto { Name = name, TenantId = tenantId };
        }

        // ═════════════════════════════════════════════
        //  CreateCategoryAsync
        // ═════════════════════════════════════════════

        [Fact]
        public async Task CreateCategoryAsync_ShouldMapNameAndTenantIdToCategory()
        {
            // Arrange
            var dto = BuildDto(name: "Electronics", tenantId: "tenant-1");
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category c) => c);

            // Act
            var result = await _sut.CreateCategoryAsync(dto);

            // Assert
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.TenantId, result.TenantId);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCallAddAsyncExactlyOnce()
        {
            // Arrange
            var dto = BuildDto();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync(new Category());

            // Act
            await _sut.CreateCategoryAsync(dto);

            // Assert
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCallSaveChangesAsyncExactlyOnce()
        {
            // Arrange
            var dto = BuildDto();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync(new Category());

            // Act
            await _sut.CreateCategoryAsync(dto);

            // Assert
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCallSaveChanges_AfterAddAsync()
        {
            // Arrange — track call order
            var callOrder = new List<string>();
            var dto = BuildDto();

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Category>()))
                .Callback(() => callOrder.Add("AddAsync"))
                .ReturnsAsync(new Category());

            _uowMock
                .Setup(u => u.SaveChangesAsync())
                .Callback(() => callOrder.Add("SaveChanges"))
                .Returns(Task.FromResult(1));

            // Act
            await _sut.CreateCategoryAsync(dto);

            // Assert
            Assert.Equal(new[] { "AddAsync", "SaveChanges" }, callOrder);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldReturnCategoryFromRepository()
        {
            // Arrange
            var dto = BuildDto();
            var expectedCategory = new Category { Id = 5, Name = dto.Name };

            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Category>()))
                .ReturnsAsync(expectedCategory);

            // Act
            var result = await _sut.CreateCategoryAsync(dto);

            // Assert
            Assert.Equal(expectedCategory, result);
        }

        // ═════════════════════════════════════════════
        //  DeleteCategoryAsync
        // ═════════════════════════════════════════════

        [Fact]
        public async Task DeleteCategoryAsync_ShouldCallDeleteWithCorrectId()
        {
            // Arrange
            const int targetId = 3;
            _repoMock.Setup(r => r.Delete(targetId)).ReturnsAsync(true);

            // Act
            await _sut.DeleteCategoryAsync(targetId);

            // Assert
            _repoMock.Verify(r => r.Delete(targetId), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldCallSaveChangesAsyncExactlyOnce()
        {
            // Arrange
            _repoMock.Setup(r => r.Delete(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            await _sut.DeleteCategoryAsync(1);

            // Assert
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            // Arrange
            _repoMock.Setup(r => r.Delete(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteCategoryAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange — category not found
            _repoMock.Setup(r => r.Delete(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteCategoryAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldCallSaveChanges_AfterDelete()
        {
            // Arrange — track call order
            var callOrder = new List<string>();

            _repoMock
                .Setup(r => r.Delete(It.IsAny<int>()))
                .Callback(() => callOrder.Add("Delete"))
                .ReturnsAsync(true);

            _uowMock
                .Setup(u => u.SaveChangesAsync())
                .Callback(() => callOrder.Add("SaveChanges"))
                .Returns(Task.FromResult(1));

            // Act
            await _sut.DeleteCategoryAsync(1);

            // Assert
            Assert.Equal(new[] { "Delete", "SaveChanges" }, callOrder);
        }

        // ═════════════════════════════════════════════
        //  GetCategoryById
        // ═════════════════════════════════════════════

        [Fact]
        public async Task GetCategoryById_ShouldCallGetByIdAsyncWithCorrectId()
        {
            // Arrange
            const int targetId = 7;
            _repoMock
                .Setup(r => r.GetByIdAsync(targetId))
                .ReturnsAsync(new Category { Id = targetId });

            // Act
            await _sut.GetCategoryById(targetId);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(targetId), Times.Once);
        }

        [Fact]
        public async Task GetCategoryById_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            const int targetId = 7;
            var expectedCategory = new Category { Id = targetId, Name = "Books" };

            _repoMock
                .Setup(r => r.GetByIdAsync(targetId))
                .ReturnsAsync(expectedCategory);

            // Act
            var result = await _sut.GetCategoryById(targetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCategory.Id, result.Id);
            Assert.Equal(expectedCategory.Name, result.Name);
        }

        [Fact]
        public async Task GetCategoryById_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            _repoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _sut.GetCategoryById(999);

            // Assert
            Assert.Null(result);
        }

        // ═════════════════════════════════════════════
        //  UpdateCategory
        // ═════════════════════════════════════════════

        [Fact]
        public async Task UpdateCategory_ShouldBuildCategoryWithCorrectIdAndDtoFields()
        {
            // Arrange
            const int categoryId = 4;
            var dto = BuildDto(name: "Updated Category", tenantId: "tenant-2");

            Category capturedCategory = null;
            _repoMock
                .Setup(r => r.Update(It.IsAny<Category>()))
                .Callback<Category>(c => capturedCategory = c)
                .ReturnsAsync((Category c) => c);

            // Act
            await _sut.UpdateCategory(categoryId, dto);

            // Assert
            Assert.NotNull(capturedCategory);
            Assert.Equal(categoryId, capturedCategory.Id);
            Assert.Equal(dto.Name, capturedCategory.Name);
            Assert.Equal(dto.TenantId, capturedCategory.TenantId);
        }

        [Fact]
        public async Task UpdateCategory_ShouldCallUpdateExactlyOnce()
        {
            // Arrange
            var dto = BuildDto();
            _repoMock.Setup(r => r.Update(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

            // Act
            await _sut.UpdateCategory(1, dto);

            // Assert
            _repoMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_ShouldCallSaveChangesAsyncExactlyOnce()
        {
            // Arrange
            var dto = BuildDto();
            _repoMock.Setup(r => r.Update(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

            // Act
            await _sut.UpdateCategory(1, dto);

            // Assert
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_ShouldCallSaveChanges_AfterUpdate()
        {
            // Arrange — track call order
            var callOrder = new List<string>();
            var dto = BuildDto();

            _repoMock
                .Setup(r => r.Update(It.IsAny<Category>()))
                .Callback(() => callOrder.Add("Update"))
                .ReturnsAsync((Category c) => c);

            _uowMock
                .Setup(u => u.SaveChangesAsync())
                .Callback(() => callOrder.Add("SaveChanges"))
                .Returns(Task.FromResult(1));

            // Act
            await _sut.UpdateCategory(1, dto);

            // Assert
            Assert.Equal(new[] { "Update", "SaveChanges" }, callOrder);
        }

        [Fact]
        public async Task UpdateCategory_ShouldReturnCategoryWithCorrectValues()
        {
            // Arrange
            const int categoryId = 4;
            var dto = BuildDto(name: "Updated Name", tenantId: "tenant-5");
            _repoMock.Setup(r => r.Update(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

            // Act
            var result = await _sut.UpdateCategory(categoryId, dto);

            // Assert
            Assert.Equal(categoryId, result.Id);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.TenantId, result.TenantId);
        }

        [Fact]
        public async Task UpdateCategory_ShouldNotCallAddAsync_DuringUpdate()
        {
            // Arrange
            var dto = BuildDto();
            _repoMock.Setup(r => r.Update(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

            // Act
            await _sut.UpdateCategory(1, dto);

            // Assert — update must never accidentally create a new record
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
        }
    }
}