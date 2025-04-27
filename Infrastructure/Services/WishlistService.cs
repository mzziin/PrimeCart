using Application.Common;
using Application.DTOs.Wishlist;
using Application.IServices;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    
    public WishlistService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Result<WishlistResponse>> GetWishlist(Guid customerId)
    {
        var wishlist = await _context.Wishlists
            .Include(w=>w.WishlistItems)
            .ThenInclude(w=>w.Product)
            .AsNoTracking()
            .Select(w=>new WishlistResponse
            {
                Id = w.Id,
                CustomerId = w.CustomerId,
                WishlistItems = w.WishlistItems.Select(wi=>new WishlistItemResponse()
                {
                    ProductId = wi.ProductId,
                    ProductName = wi.Product.Name
                }).ToList()
            })
            .FirstOrDefaultAsync(w => w.CustomerId == customerId);
        
        if(wishlist is null)
            return Result<WishlistResponse>.Failure("Wishlist not found", 404);
        
        return Result<WishlistResponse>.Success(wishlist);
    }

    public async Task<Result<WishlistResponse>> AddToWishlist(Guid customerId, Guid productId)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
  
        if (!productExists)
            return Result<WishlistResponse>.Failure("Product not found", 404);
        
        var wishlist = await _context.Wishlists
            .FirstOrDefaultAsync(w=>w.CustomerId == customerId);

        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId
            };
            await _context.Wishlists.AddAsync(wishlist);
        }
        
        var itemExists = await _context.WishlistItems
            .AnyAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);
        
        if (itemExists)
            return Result<WishlistResponse>.Failure("Product already exists in wishlist");

        await _context.WishlistItems.AddAsync(new WishlistItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WishlistId = wishlist.Id
        });
        var result = await _context.SaveChangesAsync();
        
        if(result == 0)
            return Result<WishlistResponse>.Failure("Failed to add product to wishlist");

        var updatedWishlist = await _context.Wishlists
            .Where(w => w.Id == wishlist.Id)
            .Include(w => w.WishlistItems)
            .ThenInclude(w => w.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        if (updatedWishlist == null)
            return Result<WishlistResponse>.Failure("Failed to retrieve updated wishlist");
        
        return Result<WishlistResponse>.Success(new WishlistResponse
        {
            CustomerId = customerId,
            Id = updatedWishlist.Id,
            WishlistItems = updatedWishlist.WishlistItems.Select(w => new WishlistItemResponse
            {
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
            }).ToList()
        });
    }

    public async Task<Result<WishlistResponse>> RemoveFromWishlist(Guid customerId, Guid productId)
    {
        var wishlist = await _context.Wishlists
            .Where(w=>w.CustomerId == customerId)
            .Include(w=>w.WishlistItems)
            .ThenInclude(w=>w.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        if (wishlist == null)
            return Result<WishlistResponse>.Failure("Wishlist not found", 404);
        
        var itemToRemove = wishlist.WishlistItems.FirstOrDefault(wi => wi.ProductId == productId);
        
        if (itemToRemove is null)
            return Result<WishlistResponse>.Failure("Product not found in wishlist");
        
        wishlist.WishlistItems.Remove(itemToRemove);
        var result = await _context.SaveChangesAsync();
        
        if(result == 0)
            return Result<WishlistResponse>.Failure("Failed to remove product from wishlist");
        
        return Result<WishlistResponse>.Success(new WishlistResponse
        {
            CustomerId = customerId,
            Id = wishlist.Id,
            WishlistItems = wishlist.WishlistItems.Select(w => new WishlistItemResponse
            {
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
            }).ToList()
        });
    }

    public async Task<Result<bool>> ClearWishlist(Guid customerId)
    {
        var wishlist = await _context.Wishlists
            .Include(w=>w.WishlistItems)
            .FirstOrDefaultAsync(w=>w.CustomerId == customerId);
        
        if (wishlist is null)
            return Result<bool>.Failure("Wishlist not found", 404);
        
        if(wishlist.WishlistItems.Count == 0)
            return Result<bool>.Success(true);
        
        _context.WishlistItems.RemoveRange(wishlist.WishlistItems);
        var result = await _context.SaveChangesAsync();
        
        if(result == 0)
            return Result<bool>.Failure("Failed to clear wishlist");
        
        return Result<bool>.Success(true);
    }
}