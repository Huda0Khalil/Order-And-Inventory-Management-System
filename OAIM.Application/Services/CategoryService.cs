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
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        public CategoryService(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public Task<Category> CreateCategoryAsync(CategoryDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name,
                TenantId = categoryDto.TenantId
            };
            var result = _categoryRepository.AddAsync(category);
            return result;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            bool result = await _categoryRepository.Delete(id);
            return result;
        }

        public async Task<PagedResult<Category>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _categoryRepository
                       .GetAll()
                       .AsNoTracking();
            var totalCount = await query.CountAsync();

            var items = await query
                        .OrderBy(p => p.Name)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
            return new PagedResult<Category>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

        }

        public async Task<Category> GetCategoryById(int id)
        {
            var result = await _categoryRepository.GetByIdAsync(id);
            return result;
        }

        public Task<Category> UpdateCategory(int id, CategoryDto categoryDto)
        {
            Category category = new Category
            {
                Id = id,
                Name = categoryDto.Name,
                TenantId = categoryDto.TenantId
            };
            var categoryUpdated =  _categoryRepository.Update(category);
            return categoryUpdated;
        }
    }
}
