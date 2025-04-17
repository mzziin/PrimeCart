using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PrimeCart.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetALlCategoriesAsync();
            
            return Ok(result.Value);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            
            if(!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            
            return StatusCode(result.StatusCode ,result.Value);
        }

        /*[HttpGet]
        public async Task<IActionResult> GetCategoryByName(string name)
        {
            var result = await _categoryService.GetCategoryByNameAsync(name);
            
            if(!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            
            return StatusCode(result.StatusCode ,result.Value);
        }*/

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] string name)
        {
            var result = await _categoryService.CreateCategoryAsync(name);
            
            if(!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            
            return StatusCode(result.StatusCode ,result.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] string name)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, name);
            
            if(!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            
            return StatusCode(result.StatusCode ,result.Value);
        }
    }
}
