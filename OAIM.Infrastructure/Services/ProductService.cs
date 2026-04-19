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
        public async Task<Product> CreateProductAsync(ProductDto createProductDto)
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

        public async Task<PagedResult<Product>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _productRepository
                       .GetAll()
                       .AsNoTracking()
                       .Include(p => p.Category)
                       .Include(p => p.Supplier);
            var totalCount = await query.CountAsync();

            var items = await query
                        .OrderBy(p => p.Name)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
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

        public async Task<Product> UpdateProduct(int id, ProductDto updateProductDto)
        {
            Product product = new Product()
            {
                Id = id,
                Name = updateProductDto.Name,
                Price = updateProductDto.Price,
                Barcode = updateProductDto.Barcode,
                StockQuantity = updateProductDto.StockQuantity,
                CategoryId = updateProductDto.CategoryId,
                SupplierId = updateProductDto.SupplierId,
                TenantId = updateProductDto.TenantId
            };
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return product;

        }
    }
}
