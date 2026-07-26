using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAIM.Application.IServices
{
    public interface IProductService
    {
        Task<PagedResult<Product>> GetAllProductsAsync(int pageNumber, int pageSize, int? CategoryId);
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(ProductDto createProductDto);
        Task<Product> UpdateProduct(int id, ProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
    }
}
