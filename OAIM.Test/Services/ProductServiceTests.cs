//using Moq;
//using Xunit;
//using OAIM.Application.Services;
//using OAIM.Application.DTO;
//using OAIM.Domain.Entities;
//using OAIM.Domain.Interfaces;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using OAIM.Application.Interfaces;

//namespace OAIM.Tests.Services
//{
//    public class ProductServiceTests
//    {
//        // ─────────────────────────────────────────────
//        //  Setup — shared mocks and SUT (System Under Test)
//        // ─────────────────────────────────────────────
//        private readonly Mock<IRepository<Product, int>> _repoMock;
//        private readonly Mock<IUnitOfWork> _uowMock;
//        private readonly ProductService _sut;

//        public ProductServiceTests()
//        {
//            _repoMock = new Mock<IRepository<Product, int>>();
//            _uowMock = new Mock<IUnitOfWork>();
//            _sut = new ProductService(_repoMock.Object, _uowMock.Object);
//        }

//        // ─────────────────────────────────────────────
//        //  Helper: builds a standard ProductDto
//        // ─────────────────────────────────────────────
//        private static ProductDto BuildDto(
//            string name = "Test Product",
//            decimal price = 9.99m,
//            string barcode = "ABC123",
//            int stock = 10,
//            int categoryId = 1,
//            int supplierId = 2,
//            string tenantId = "Tenant-3")
//        {
//            return new ProductDto
//            {
//                Name = name,
//                Price = price,
//                Barcode = barcode,
//                StockQuantity = stock,
//                CategoryId = categoryId,
//                SupplierId = supplierId,
//                TenantId = tenantId
//            };
//        }

//        // ═════════════════════════════════════════════
//        //  CreateProductAsync
//        // ═════════════════════════════════════════════

//        [Fact]
//        public async Task CreateProductAsync_ShouldMapAllDtoFieldsToProduct()
//        {
//            // Arrange
//            var dto = BuildDto();
//            _repoMock
//                .Setup(r => r.AddAsync(It.IsAny<Product>()))
//                .ReturnsAsync((Product p) => p);

//            // Act
//            var result = await _sut.CreateProductAsync(dto);

//            // Assert — every DTO field must be mapped
//            Assert.Equal(dto.Name, result.Name);
//            Assert.Equal(dto.Price, result.Price);
//            Assert.Equal(dto.Barcode, result.Barcode);
//            Assert.Equal(dto.StockQuantity, result.StockQuantity);
//            Assert.Equal(dto.CategoryId, result.CategoryId);
//            Assert.Equal(dto.SupplierId, result.SupplierId);
//            Assert.Equal(dto.TenantId, result.TenantId);
//        }

//        [Fact]
//        public async Task CreateProductAsync_ShouldCallAddAsyncExactlyOnce()
//        {
//            // Arrange
//            var dto = BuildDto();
//            _repoMock
//                .Setup(r => r.AddAsync(It.IsAny<Product>()))
//                .ReturnsAsync(new Product());

//            // Act
//            await _sut.CreateProductAsync(dto);

//            // Assert
//            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
//        }

//        [Fact]
//        public async Task CreateProductAsync_ShouldCallSaveChangesAsyncExactlyOnce()
//        {
//            // Arrange
//            var dto = BuildDto();
//            _repoMock
//                .Setup(r => r.AddAsync(It.IsAny<Product>()))
//                .ReturnsAsync(new Product());

//            // Act
//            await _sut.CreateProductAsync(dto);

//            // Assert
//            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task CreateProductAsync_ShouldCallSaveChanges_AfterAddAsync()
//        {
//            // Arrange — track call order
//            var callOrder = new List<string>();
//            var dto = BuildDto();

//            _repoMock
//                .Setup(r => r.AddAsync(It.IsAny<Product>()))
//                .Callback(() => callOrder.Add("AddAsync"))
//                .ReturnsAsync(new Product());

//            _uowMock
//                .Setup(u => u.SaveChangesAsync())
//                .Callback(() => callOrder.Add("SaveChanges"))
//                .Returns((Task.FromResult(1)));

//            // Act
//            await _sut.CreateProductAsync(dto);

//            // Assert
//            Assert.Equal(new[] { "AddAsync", "SaveChanges" }, callOrder);
//        }

//        [Fact]
//        public async Task CreateProductAsync_ShouldReturnProductFromRepository()
//        {
//            // Arrange
//            var dto = BuildDto();
//            var expectedProduct = new Product { Id = 42, Name = dto.Name };

//            _repoMock
//                .Setup(r => r.AddAsync(It.IsAny<Product>()))
//                .ReturnsAsync(expectedProduct);

//            // Act
//            var result = await _sut.CreateProductAsync(dto);

//            // Assert
//            Assert.Equal(expectedProduct, result);
//        }

//        // ═════════════════════════════════════════════
//        //  DeleteProductAsync
//        // ═════════════════════════════════════════════

//        [Fact]
//        public async Task DeleteProductAsync_ShouldCallDeleteWithCorrectId()
//        {
//            // Arrange
//            const int targetId = 7;
//            _repoMock.Setup(r => r.Delete(targetId)).ReturnsAsync(true);

//            // Act
//            await _sut.DeleteProductAsync(targetId);

//            // Assert
//            _repoMock.Verify(r => r.Delete(targetId), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteProductAsync_ShouldCallSaveChangesAsyncExactlyOnce()
//        {
//            // Arrange
//            _repoMock.Setup(r => r.Delete(It.IsAny<int>())).ReturnsAsync(true);

//            // Act
//            await _sut.DeleteProductAsync(1);

//            // Assert
//            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteProductAsync_ShouldReturnTrue_WhenRepositoryReturnsTrue()
//        {
//            // Arrange
//            _repoMock.Setup(r => r.Delete(It.IsAny<int>())).ReturnsAsync(true);

//            // Act
//            var result = await _sut.DeleteProductAsync(1);

//            // Assert
//            Assert.True(result);
//        }

//        [Fact]
//        public async Task DeleteProductAsync_ShouldReturnFalse_WhenRepositoryReturnsFalse()
//        {
//            // Arrange — product not found in repo
//            _repoMock.Setup(r => r.Delete(It.IsAny<int>())).ReturnsAsync(false);

//            // Act
//            var result = await _sut.DeleteProductAsync(99);

//            // Assert
//            Assert.False(result);
//        }

//        [Fact]
//        public async Task DeleteProductAsync_ShouldCallSaveChanges_AfterDelete()
//        {
//            // Arrange — track call order
//            var callOrder = new List<string>();

//            _repoMock
//                .Setup(r => r.Delete(It.IsAny<int>()))
//                .Callback(() => callOrder.Add("Delete"))
//                .ReturnsAsync(true);

//            _uowMock
//                .Setup(u => u.SaveChangesAsync())
//                .Callback(() => callOrder.Add("SaveChanges"))
//                .Returns((Task.FromResult(1)));

//            // Act
//            await _sut.DeleteProductAsync(1);

//            // Assert
//            Assert.Equal(new[] { "Delete", "SaveChanges" }, callOrder);
//        }

//        // ═════════════════════════════════════════════
//        //  GetAllProductsAsync
//        // ═════════════════════════════════════════════

//        private static IQueryable<Product> BuildProductList(int count)
//        {
//            return Enumerable.Range(1, count)
//                .Select(i => new Product
//                {
//                    Id = i,
//                    Name = $"Product {i:D3}"   // "Product 001", "Product 002" ...
//                })
//                .AsQueryable();
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldCapPageSizeAt100_WhenGivenLargerValue()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(150));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 1, pageSize: 200);

//            // Assert
//            Assert.Equal(100, result.PageSize);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldKeepPageSize_WhenBelowOrEqualTo100()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(50));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 1, pageSize: 50);

//            // Assert
//            Assert.Equal(50, result.PageSize);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldCallGetAll_WithCategoryAndSupplierIncludes()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(5));

//            // Act
//            await _sut.GetAllProductsAsync(pageNumber: 1, pageSize: 10);

//            // Assert
//            _repoMock.Verify(r => r.GetAll(
//                It.Is<string[]>(includes =>
//                    includes.Contains(nameof(Product.Category)) &&
//                    includes.Contains(nameof(Product.Supplier)))),
//                Times.Once);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldReturnCorrectTotalCount()
//        {
//            // Arrange — 25 products total, page size 10
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(25));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 1, pageSize: 10);

//            // Assert
//            Assert.Equal(25, result.TotalCount);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_TotalCount_ShouldReflectFullDataset_NotJustPage()
//        {
//            // Arrange — 25 products, page 2, size 10 → only 10 items but total is 25
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(25));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 2, pageSize: 10);

//            // Assert
//            Assert.Equal(25, result.TotalCount);    // total never changes
//            Assert.Equal(10, result.Items.Count);   // page still has 10
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldReturnCorrectItemsCount_ForFirstPage()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(25));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 1, pageSize: 10);

//            // Assert
//            Assert.Equal(10, result.Items.Count);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldReturnRemainingItems_OnLastPage()
//        {
//            // Arrange — 25 products, page 3 of size 10 → only 5 left
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(25));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 3, pageSize: 10);

//            // Assert
//            Assert.Equal(5, result.Items.Count);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldReturnItemsOrderedByName()
//        {
//            // Arrange — add out-of-order names
//            var unordered = new List<Product>
//            {
//                new Product { Id = 1, Name = "Zebra" },
//                new Product { Id = 2, Name = "Apple" },
//                new Product { Id = 3, Name = "Mango" }
//            }.AsQueryable();

//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(unordered);

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 1, pageSize: 10);

//            // Assert
//            var names = result.Items.Select(p => p.Name).ToList();
//            Assert.Equal(new[] { "Apple", "Mango", "Zebra" }, names);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldReturnCorrectPageNumber()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(20));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 2, pageSize: 10);

//            // Assert
//            Assert.Equal(2, result.PageNumber);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldReturnEmptyItems_WhenNoProducts()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(Enumerable.Empty<Product>().AsQueryable());

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 1, pageSize: 10);

//            // Assert
//            Assert.Empty(result.Items);
//            Assert.Equal(0, result.TotalCount);
//        }

//        [Fact]
//        public async Task GetAllProductsAsync_ShouldReturnCorrectSecondPage()
//        {
//            // Arrange — products named "Product 001" to "Product 015"
//            _repoMock
//                .Setup(r => r.GetAll(It.IsAny<string[]>()))
//                .Returns(BuildProductList(15));

//            // Act
//            var result = await _sut.GetAllProductsAsync(pageNumber: 2, pageSize: 10);

//            // Assert — page 2 should have exactly 5 items
//            Assert.Equal(5, result.Items.Count);
//            // and none of them should be from page 1
//            Assert.DoesNotContain(result.Items, p => p.Name == "Product 001");
//        }

//        // ═════════════════════════════════════════════
//        //  GetProductByIdAsync
//        // ═════════════════════════════════════════════

//        [Fact]
//        public async Task GetProductByIdAsync_ShouldCallGetByIdAsyncWithCorrectId()
//        {
//            // Arrange
//            const int targetId = 5;
//            _repoMock
//                .Setup(r => r.GetByIdAsync(targetId))
//                .ReturnsAsync(new Product { Id = targetId });

//            // Act
//            await _sut.GetProductByIdAsync(targetId);

//            // Assert
//            _repoMock.Verify(r => r.GetByIdAsync(targetId), Times.Once);
//        }

//        [Fact]
//        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
//        {
//            // Arrange
//            const int targetId = 5;
//            var expectedProduct = new Product { Id = targetId, Name = "Found Product" };

//            _repoMock
//                .Setup(r => r.GetByIdAsync(targetId))
//                .ReturnsAsync(expectedProduct);

//            // Act
//            var result = await _sut.GetProductByIdAsync(targetId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(expectedProduct.Id, result.Id);
//            Assert.Equal(expectedProduct.Name, result.Name);
//        }

//        [Fact]
//        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
//        {
//            // Arrange
//            _repoMock
//                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
//                .ReturnsAsync((Product)null);

//            // Act
//            var result = await _sut.GetProductByIdAsync(999);

//            // Assert
//            Assert.Null(result);
//        }

//        // ═════════════════════════════════════════════
//        //  UpdateProduct
//        // ═════════════════════════════════════════════

//        [Fact]
//        public async Task UpdateProduct_ShouldBuildProductWithCorrectIdAndAllDtoFields()
//        {
//            // Arrange
//            const int productId = 10;
//            var dto = BuildDto(
//                name: "Updated Name",
//                price: 19.99m,
//                barcode: "XYZ999",
//                stock: 20,
//                categoryId: 4,
//                supplierId: 5,
//                tenantId: "Tenant-6");

//            Product capturedProduct = null;
//            _repoMock
//                .Setup(r => r.Update(It.IsAny<Product>()))
//                .Callback<Product>(p => capturedProduct = p);

//            // Act
//            await _sut.UpdateProduct(productId, dto);

//            // Assert — the entity sent to repo must have correct id + all fields
//            Assert.NotNull(capturedProduct);
//            Assert.Equal(productId, capturedProduct.Id);
//            Assert.Equal(dto.Name, capturedProduct.Name);
//            Assert.Equal(dto.Price, capturedProduct.Price);
//            Assert.Equal(dto.Barcode, capturedProduct.Barcode);
//            Assert.Equal(dto.StockQuantity, capturedProduct.StockQuantity);
//            Assert.Equal(dto.CategoryId, capturedProduct.CategoryId);
//            Assert.Equal(dto.SupplierId, capturedProduct.SupplierId);
//            Assert.Equal(dto.TenantId, capturedProduct.TenantId);
//        }

//        [Fact]
//        public async Task UpdateProduct_ShouldCallUpdateExactlyOnce()
//        {
//            // Arrange
//            var dto = BuildDto();

//            // Act
//            await _sut.UpdateProduct(1, dto);

//            // Assert
//            _repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateProduct_ShouldCallSaveChangesAsyncExactlyOnce()
//        {
//            // Arrange
//            var dto = BuildDto();

//            // Act
//            await _sut.UpdateProduct(1, dto);

//            // Assert
//            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateProduct_ShouldCallSaveChanges_AfterUpdate()
//        {
//            // Arrange — track call order
//            var callOrder = new List<string>();
//            var dto = BuildDto();

//            _repoMock
//                .Setup(r => r.Update(It.IsAny<Product>()))
//                .Callback(() => callOrder.Add("Update"));

//            _uowMock
//                .Setup(u => u.SaveChangesAsync())
//                .Callback(() => callOrder.Add("SaveChanges"))
//                .Returns((Task.FromResult(1)));

//            // Act
//            await _sut.UpdateProduct(1, dto);

//            // Assert
//            Assert.Equal(new[] { "Update", "SaveChanges" }, callOrder);
//        }

//        [Fact]
//        public async Task UpdateProduct_ShouldReturnProductWithCorrectValues()
//        {
//            // Arrange
//            const int productId = 10;
//            var dto = BuildDto(name: "Updated", price: 55.00m);

//            // Act
//            var result = await _sut.UpdateProduct(productId, dto);

//            // Assert
//            Assert.Equal(productId, result.Id);
//            Assert.Equal(dto.Name, result.Name);
//            Assert.Equal(dto.Price, result.Price);
//        }

//        [Fact]
//        public async Task UpdateProduct_ShouldNotCallAddAsync_DuringUpdate()
//        {
//            // Arrange
//            var dto = BuildDto();

//            // Act
//            await _sut.UpdateProduct(1, dto);

//            // Assert — update must never create a new record
//            _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
//        }
//    }
//}