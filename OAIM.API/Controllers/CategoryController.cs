using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAIM.Application.DTO;

namespace OAIM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto)
        {
            try
            {
                var result = await _categoryService.CreateCategoryAsync(categoryDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _categoryService.GetAllCategoriesAsync(pageNumber,pageSize);
            if (result == null)
            {
                return NotFound("No categories found.");
            }
            return Ok(result);
        }
        [HttpGet("getList")]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _categoryService.GetListCategory();
            if (result == null)
            {
                return NotFound("No categories found.");
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _categoryService.GetCategoryById(id);
            if (result == null)
            {
                return NotFound($"Category with Id {id} was not found.");
            }
            return Ok(result);
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto categoryDto)
        {
            try
            {
                var result = await _categoryService.UpdateCategory(id, categoryDto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound($"Category with Id {id} was not found.");
            }
            return Ok(new
            {
                Message = $"Category with id ${id} deleted"
            });
        }

        }
}
