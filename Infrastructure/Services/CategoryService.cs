using Application.Common;
using Application.DTOs;
using Application.IServices;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CategoryResponse>>> GetALlCategoriesAsync()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
            })
            .ToListAsync();

        return Result<List<CategoryResponse>>.Success(categories);
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            }).FirstOrDefaultAsync();

        if (category is null)
            return Result<CategoryResponse>.Failure("Category not found");

        return Result<CategoryResponse>.Success(category);
    }

    public async Task<Result<CategoryResponse>> GetCategoryByNameAsync(string name)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Name == name.ToLower())
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            }).FirstOrDefaultAsync();

        if (category is null)
            return Result<CategoryResponse>.Failure("Category not found");

        return Result<CategoryResponse>.Success(category);
    }

    public async Task<Result<CategoryResponse>> CreateCategoryAsync(string name)
    {
        var category = await _context.Categories.AddAsync(new Category
        {
            Id = Guid.NewGuid(),
            Name = name.ToLower()
        });
        var result = await _context.SaveChangesAsync();

        if (result == 0)
            return Result<CategoryResponse>.Failure("Category not created");

        return Result<CategoryResponse>.Success(new CategoryResponse
        {
            Id = category.Entity.Id,
            Name = category.Entity.Name
        }, 201);
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(Guid id, string name)
    {
        var category = await _context.Categories.FindAsync(id);
        if(category is null)
            return Result<CategoryResponse>.Failure("Category not found", 404);
        
        category.Name = name;
        var result = await _context.SaveChangesAsync();
        
        if(result != 1)
            return Result<CategoryResponse>.Failure("Category not updated");

        return Result<CategoryResponse>.Success(new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name
        }, 201);
    }
}