using Microsoft.EntityFrameworkCore;
using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Application.IServices;
using OAIM.Domain.Entities;
using OAIM.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OAIM.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product, int> _productRepository;
        private IUnitOfWork _unitOfWork;

        public ProductService(IRepository<Product, int> productRepository, IUnitOfWork unitOfWork) 
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Product> CreateProductAsync(ProductDto
            createProductDto)
        {
            var product = new Product();
            product.Name = createProductDto.Name;
            product.Price = createProductDto.Price;
            product.Barcode = createProductDto.Barcode;
            product.StockQuantity = createProductDto.StockQuantity;
            product.CategoryId = createProductDto.CategoryId;
            product.SupplierId = createProductDto.SupplierId;
            product.TenantId = createProductDto.TenantId;
            var result = await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            bool result = await _productRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<PagedResult<Product>> GetAllProductsAsync(int pageNumber, int pageSize, int? CategoryId)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _productRepository
            .GetAll(includes: new[] { nameof(Product.Category), nameof(Product.Supplier) }).AsNoTracking();

            if (CategoryId != 0)
            {
                query = query.Where(x => x.CategoryId == CategoryId);
            }

            var totalCount = query.Count();

            var items = query
                        .OrderBy(p => p.Name)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var result = await _productRepository.GetByIdAsync(id);
            return result;
        }

        public async Task<Product> UpdateProduct(int id, ProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new KeyNotFoundException($"Product with Id {id} was not found.");
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Barcode = dto.Barcode;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.SupplierId = dto.SupplierId;
            product.TenantId = dto.TenantId;
            
            //await _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return product;

        }
    }
}
