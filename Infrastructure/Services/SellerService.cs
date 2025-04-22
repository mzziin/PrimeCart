using Application.Common;
using Application.DTOs.Seller;
using Application.IServices;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SellerService : ISellerService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public SellerService(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;   
    }

    public async Task<Result<SellerResponse>> GetSellerById(Guid id)
    {
        var seller = await _context.Sellers
            .AsNoTracking()
            .Where(s => s.User.IsActive && s.Id == id)
            .Select(s => new SellerResponse
            {
                Id = s.Id,
                Name = s.Name,
                Phone = s.Phone,
                StoreName = s.StoreName,
            })
            .FirstOrDefaultAsync();

        if (seller is null)
            return Result<SellerResponse>.Failure("Seller not found", 404);

        return Result<SellerResponse>.Success(seller);
    }

    public async Task<Result<SellerResponse>> UpdateSeller(Guid id, UpdateSellerRequest? updateSellerRequest)
    {
        var seller = await _context.Sellers
            .Where(s=>s.User.IsActive)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (seller is null)
            return Result<SellerResponse>.Failure("Seller not found", 404);
        
        bool isModified = false;
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            if (!string.IsNullOrEmpty(updateSellerRequest?.Name))
            {
                seller.Name = updateSellerRequest.Name;
                isModified = true;
            }

            if (!string.IsNullOrEmpty(updateSellerRequest?.StoreName))
            {
                seller.StoreName = updateSellerRequest.StoreName;
                isModified = true;
            }

            if (!string.IsNullOrEmpty(updateSellerRequest?.Phone) && updateSellerRequest.Phone != seller.Phone)
            {
                var phoneExist = await _context.Sellers
                    .Where(u => u.User.IsActive)
                    .Where(u => u.Id != id)
                    .AnyAsync(u => u.Phone == updateSellerRequest.Phone);

                if (phoneExist)
                    return Result<SellerResponse>.Failure("Phone already exists", 409);

                seller.Phone = updateSellerRequest.Phone;
                isModified = true;
            }

            if (!string.IsNullOrEmpty(updateSellerRequest.Password) &&
                !string.IsNullOrEmpty(updateSellerRequest.ConfirmPassword))
            {
                var passwordHash = _passwordHasher.HashPassword(updateSellerRequest.Password);
                seller.User.PasswordHash = passwordHash;
                isModified = true;
            }

            if (!isModified)
            {
                await transaction.CommitAsync();
                return Result<SellerResponse>.Success(new SellerResponse
                {
                    Id = seller.Id,
                    Name = seller.Name,
                    StoreName = seller.StoreName,
                    Phone = seller.Phone
                });
            }
            
            var result = await _context.SaveChangesAsync();
            if (result == 0)
            {
                await transaction.RollbackAsync();
                return Result<SellerResponse>.Failure("Failed to update seller");
            }
                
            await transaction.CommitAsync();
            
            var response = new SellerResponse
            {
                Id = seller.Id,
                Name = seller.Name,
                StoreName = seller.StoreName,
                Phone = seller.Phone
            };
            
            return Result<SellerResponse>.Success(response);
            
        }catch(Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<SellerResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteSeller(Guid id)
    {
        var seller = await _context.Sellers
            .Where(s => s.User.IsActive && s.Id == id)
            .Include(s => s.User)
            .FirstOrDefaultAsync();

        if (seller is null)
            return Result<bool>.Failure("Seller not found", 404);

        seller.User.IsActive = false;
        var result = await _context.SaveChangesAsync();
        
        if (result == 0)
            return Result<bool>.Failure("Failed to delete seller");

        return Result<bool>.Success(true);
    }
}