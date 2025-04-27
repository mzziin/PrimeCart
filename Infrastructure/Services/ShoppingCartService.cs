using Application.Common;
using Application.DTOs.ShoppingCart;
using Application.IServices;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly AppDbContext _context;

    public ShoppingCartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ShoppingCartResponse>> GetCart(Guid customerId)
    {
        var shoppingCart = await _context.ShoppingCarts
            .Where(s => s.CustomerId == customerId)
            .AsNoTracking()
            .Select(s => new ShoppingCartResponse
            {
                Id = s.Id,
                CustomerId = customerId,
                ShoppingCartItems = s.ShoppingCartItems.Select(sc => new ShoppingCartItemResponse
                {
                    Id = sc.Id,
                    ProductId = sc.ProductId,
                    ProductName = sc.Product.Name,
                    Quantity = sc.Quantity,
                    UnitPrice = sc.Product.Price,
                    TotalPrice = sc.Product.Price * sc.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (shoppingCart is null)
            return Result<ShoppingCartResponse>.Failure("Shopping cart not found", 404);

        return Result<ShoppingCartResponse>.Success(shoppingCart);
    }

    public async Task<Result<ShoppingCartResponse>> AddToCart(Guid customerId, Guid productId, int quantity = 1)
    {
        if (quantity <= 0)
            return Result<ShoppingCartResponse>.Failure("Quantity must be greater than 0", 400);
        
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
            return Result<ShoppingCartResponse>.Failure("Product not found", 404);

        var shoppingCart = await _context.ShoppingCarts
            .Include(s => s.ShoppingCartItems)
            .ThenInclude(shoppingCartItem => shoppingCartItem.Product)
            .FirstOrDefaultAsync(s => s.CustomerId == customerId);

        if (shoppingCart is null)
        {
            shoppingCart = new ShoppingCart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ShoppingCartItems = new List<ShoppingCartItem>()
            };
            await _context.ShoppingCarts.AddAsync(shoppingCart);
        }

        var itemExists = shoppingCart.ShoppingCartItems
            .FirstOrDefault(sc => sc.ProductId == productId);

        if (itemExists is null)
            return Result<ShoppingCartResponse>.Failure("Product already exists in cart");

        await _context.ShoppingCartItems.AddAsync(new ShoppingCartItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            CartId = shoppingCart.Id,
            Product = product
        });

        var result = await _context.SaveChangesAsync();
        if (result == 0)
            return Result<ShoppingCartResponse>.Failure("Failed to add product to cart");
        
        return Result<ShoppingCartResponse>.Success(new ShoppingCartResponse
        {
            Id = shoppingCart.Id,
            CustomerId = customerId,
            ShoppingCartItems = shoppingCart.ShoppingCartItems.Select(sc => new ShoppingCartItemResponse
            {
                Id = sc.Id,
                ProductId = sc.ProductId,
                ProductName = sc.Product.Name,
                Quantity = sc.Quantity,
                UnitPrice = sc.Product.Price,
                TotalPrice = sc.Product.Price * sc.Quantity
            }).ToList()
        });
    }

    public async Task<Result<ShoppingCartResponse>> UpdateCart(Guid customerId, Guid productId, int quantity)
    {
        if (quantity <= 0)
            return Result<ShoppingCartResponse>.Failure("Quantity must be greater than 0", 400);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null)
            return Result<ShoppingCartResponse>.Failure("Product not found", 404);
        
        if (product.Stock < quantity)
            return Result<ShoppingCartResponse>.Failure("Requested quantity exceeds available stock", 400);
        
        var shoppingCart = await _context.ShoppingCarts
            .Include(shoppingCart => shoppingCart.ShoppingCartItems)
            .ThenInclude(shoppingCartItem => shoppingCartItem.Product)
            .FirstOrDefaultAsync(s => s.CustomerId == customerId);
        if (shoppingCart is null)
            return Result<ShoppingCartResponse>.Failure("Shopping cart not found", 404);

        var itemToUpdate = shoppingCart.ShoppingCartItems.FirstOrDefault(sc => sc.ProductId == productId);
        if (itemToUpdate is null)
            return Result<ShoppingCartResponse>.Failure("Product not found in cart", 404);

        itemToUpdate.Quantity = quantity;
        var result = await _context.SaveChangesAsync();

        if (result == 0)
            return Result<ShoppingCartResponse>.Failure("Failed to update cart");
        
        return Result<ShoppingCartResponse>.Success(new ShoppingCartResponse
        {
            Id = shoppingCart.Id,
            CustomerId = shoppingCart.CustomerId,
            ShoppingCartItems = shoppingCart.ShoppingCartItems.Select(sc => new ShoppingCartItemResponse
            {
                Id = sc.Id,
                ProductId = sc.ProductId,
                ProductName = sc.Product.Name,
                Quantity = sc.Quantity,
                UnitPrice = sc.Product.Price,
                TotalPrice = sc.Product.Price * sc.Quantity
            }).ToList()
        });
    }

    public async Task<Result<ShoppingCartResponse>> RemoveFromCart(Guid customerId, Guid productId)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
        if (!customerExists)
            return Result<ShoppingCartResponse>.Failure("Customer not found", 404);
        
        var shoppingCart = await _context.ShoppingCarts
            .Include(shoppingCart => shoppingCart.ShoppingCartItems)
            .ThenInclude(shoppingCartItem => shoppingCartItem.Product)
            .FirstOrDefaultAsync(s => s.CustomerId == customerId);
        if (shoppingCart is null)
            return Result<ShoppingCartResponse>.Failure("Shopping cart not found", 404);
        
        var itemToRemove = shoppingCart.ShoppingCartItems.FirstOrDefault(sc => sc.ProductId == productId);
        if (itemToRemove is null)
            return Result<ShoppingCartResponse>.Failure("Product not found in cart", 404);
        
        shoppingCart.ShoppingCartItems.Remove(itemToRemove);
        var result = await _context.SaveChangesAsync();
        
        if (result == 0)
            return Result<ShoppingCartResponse>.Failure("Failed to remove product from cart");
        
        return Result<ShoppingCartResponse>.Success(new ShoppingCartResponse
        {
            Id = shoppingCart.Id,
            CustomerId = customerId,
            ShoppingCartItems = shoppingCart.ShoppingCartItems.Select(sc => new ShoppingCartItemResponse
            {
                Id = sc.Id,
                ProductId = sc.ProductId,
                ProductName = sc.Product.Name,
                Quantity = sc.Quantity,
                UnitPrice = sc.Product.Price,
                TotalPrice = sc.Product.Price * sc.Quantity
            }).ToList()
        });
    }

    public async Task<Result<bool>> ClearCart(Guid customerId)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
        if (!customerExists)
            return Result<bool>.Failure("Customer not found", 404);
        
        var shoppingCart = await _context.ShoppingCarts
            .Include(shoppingCart => shoppingCart.ShoppingCartItems)
            .FirstOrDefaultAsync(s => s.CustomerId == customerId);
        if (shoppingCart is null)
            return Result<bool>.Failure("Shopping cart not found", 404);
        
        if(shoppingCart.ShoppingCartItems.Count == 0)
            return Result<bool>.Success(true);

        int itemCount = shoppingCart.ShoppingCartItems.Count;
        
        _context.ShoppingCartItems.RemoveRange(shoppingCart.ShoppingCartItems);
        var result = await _context.SaveChangesAsync();
        
        if (result != itemCount)
            return Result<bool>.Failure("Failed to clear all items from cart");
    
        return Result<bool>.Success(true);
    }
}