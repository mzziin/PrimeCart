using Application.Common;
using Application.DTOs;

namespace Application.IServices;

public interface ICategoryService
{
    public Task<Result<List<CategoryResponse>>> GetALlCategoriesAsync();
    public Task<Result<CategoryResponse>> GetCategoryByIdAsync(Guid id);
    public Task<Result<CategoryResponse>> GetCategoryByNameAsync(string name);
    public Task<Result<CategoryResponse>> CreateCategoryAsync(string name);
    public Task<Result<CategoryResponse>> UpdateCategoryAsync(Guid id, string name);
}