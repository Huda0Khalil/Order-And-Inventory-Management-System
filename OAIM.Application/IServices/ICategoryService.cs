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
    public interface ICategoryService
    {
        Task<Category> GetCategoryById(int id);
        Task<PagedResult<Category>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        public Task<List<Category>> GetListCategory();
                Task<Category> CreateCategoryAsync(CategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<Category> UpdateCategory(int id, CategoryDto categoryDto);
    }
}
