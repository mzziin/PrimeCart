using System.Security.Claims;
using Application.DTOs.Product;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrimeCart.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var response = await _productService.GetAllProductsAsync(1, 10);

            if (!response.IsSuccess)
                return StatusCode(response.StatusCode, response.Error);

            return StatusCode(response.StatusCode, response.Value);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var response = await _productService.GetProductByIdAsync(id);

            if (!response.IsSuccess)
                return StatusCode(response.StatusCode ,response.Error);

            return StatusCode(response.StatusCode ,response.Value);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductRequest createProduct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            createProduct.SellerId = userId;

            var response = await _productService.AddProductAsync(createProduct);

            if (!response.IsSuccess)
                return StatusCode(response.StatusCode, response.Error);

            return StatusCode(response.StatusCode ,response.Value);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest updateProduct)
        {
            var response = await _productService.UpdateProductAsync(id, updateProduct);
            
            if(!response.IsSuccess)
                return StatusCode(response.StatusCode, response.Error);
            
            return StatusCode(response.StatusCode ,response.Value);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var response = await _productService.DeleteProductAsync(id);

            if (!response.IsSuccess)
                return StatusCode(response.StatusCode ,response.Error);

            return StatusCode(response.StatusCode ,response.Value);
        }
    }
}