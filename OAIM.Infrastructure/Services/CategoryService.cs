using OAIM.Application.Data;
using OAIM.Application.DTO;
using OAIM.Domain.Interfaces;

namespace OAIM.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category,int> _categoryRepository;
        private IUnitOfWork _unitOfWork;

        public CategoryService(IRepository<Category, int> categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Category> CreateCategoryAsync(CategoryDto categoryDto)
        {
            Category category = new Category
            {
                Name = categoryDto.Name,
                TenantId = categoryDto.TenantId
            };
            var result = await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return result;
        }

       

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            bool result = await _categoryRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<PagedResult<Category>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var query = _categoryRepository
                       .GetAll(null)
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
        public async Task<List<Category>> GetListCategory()
        {
            return await _categoryRepository
                .GetAll(null)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }


        public async Task<Category> GetCategoryById(int id)
        {
            var result = await _categoryRepository.GetByIdAsync(id);
            return result;
        }

        public async Task<Category> UpdateCategory(int id, CategoryDto categoryDto)
        {
            Category category = new Category
            {
                Id = id,
                Name = categoryDto.Name,
                TenantId = categoryDto.TenantId
            };
            await _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }        
    }
}
