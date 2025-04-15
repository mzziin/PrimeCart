using Application.Common;
using Application.DTOs.Product;
using Domain.Models;

namespace Application.IServices;

public interface IProductService
{
    Task<Result<List<ProductResponse>>> GetAllProductsAsync(int pageNumber, int pageSize);
    Task<Result<ProductResponse?>> GetProductByIdAsync(Guid id);
    Task<Result<ProductResponse>> AddProductAsync(CreateProductRequest createProduct);
    Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest updateProduct);
    Task<Result<bool>> DeleteProductAsync(Guid id);
}