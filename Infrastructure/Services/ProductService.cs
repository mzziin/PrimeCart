using System.Security.Claims;
using Application.Common;
using Application.DTOs.Product;
using Application.IServices;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductResponse>>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CreatedAt = p.CreatedAt,
                SellerId = p.SellerId,
                CategoryId = p.CategoryId
            })
            .ToListAsync();

        return Result<List<ProductResponse>>.Success(products);
    }

    public async Task<Result<ProductResponse?>> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CreatedAt = p.CreatedAt,
                SellerId = p.SellerId,
                CategoryId = p.CategoryId
            })
            .FirstOrDefaultAsync();
        
        if(product is null)
            return Result<ProductResponse?>.Failure("Product not found", 404);
        
        return Result<ProductResponse?>.Success(product);
    }

    public async Task<Result<ProductResponse>> AddProductAsync(CreateProductRequest createProduct)
    {
        var productToAdd = new Product
        {
            Id = Guid.NewGuid(),
            Name = createProduct.Name,
            Description = createProduct.Description,
            Price = createProduct.Price,
            Stock = createProduct.Stock,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = createProduct.CategoryId,
            SellerId = createProduct.SellerId,
            IsDeleted = false,
        };
        
        await _context.Products.AddAsync(productToAdd);
        var result = await _context.SaveChangesAsync();
        
        if(result == 0)
            return Result<ProductResponse>.Failure("Failed to add product");
        
        var response = new ProductResponse
        {
            Id = productToAdd.Id,
            Name = productToAdd.Name,
            Description = productToAdd.Description,
            Price = productToAdd.Price,
            Stock = productToAdd.Stock,
            CategoryId = productToAdd.CategoryId,
            CreatedAt = productToAdd.CreatedAt,
            SellerId = productToAdd.SellerId
        };

        return Result<ProductResponse>.Success(response, 201);
    }

    public async Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest updateProduct)
    {
        var productFromDb = await _context.Products.FindAsync(id);
        
        if(productFromDb == null || productFromDb.IsDeleted)
            return Result<ProductResponse>.Failure("Product not found", 404);
        
        productFromDb.Name = string.IsNullOrEmpty(updateProduct.Name) ? productFromDb.Name : updateProduct.Name;
        productFromDb.Description = string.IsNullOrEmpty(updateProduct.Description) ? productFromDb.Description : updateProduct.Description;
        productFromDb.Price = updateProduct.Price == 0 ? productFromDb.Price : updateProduct.Price;
        productFromDb.Stock = updateProduct.Stock;
        
        if (Guid.TryParse(updateProduct.CategoryId.ToString(), out Guid categoryId) && categoryId != Guid.Empty)
            productFromDb.CategoryId = categoryId;
        else
            productFromDb.CategoryId = productFromDb.CategoryId;

        var result = await _context.SaveChangesAsync();
        
        if(result == 0)
            return Result<ProductResponse>.Failure("Failed to update product");

        var response = new ProductResponse
        {
            Id = productFromDb.Id,
            Name = productFromDb.Name,
            Description = productFromDb.Description,
            Price = productFromDb.Price,
            Stock = productFromDb.Stock,
            CategoryId = productFromDb.CategoryId,
            SellerId = productFromDb.SellerId,
            CreatedAt = productFromDb.CreatedAt
        };
        
        return Result<ProductResponse>.Success(response);
    }

    public async Task<Result<bool>> DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null || product.IsDeleted)
            return Result<bool>.Failure("Product not found", 404);
        
        product.IsDeleted = true;
        var result = await _context.SaveChangesAsync();
        
        if(result == 0)
            return Result<bool>.Failure("Failed to delete product");
        
        return Result<bool>.Success(true);
    }
}