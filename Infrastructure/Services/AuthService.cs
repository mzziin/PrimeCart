using Application.DTOs;
using Application.IServices;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(AppDbContext appDbContext, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _context = appDbContext;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<int> RegisterCustomer(RegisterCustomer registerCustomer)
    {
        var userExist = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.Email == registerCustomer.Email && e.IsActive);
        if (userExist is not null) return 0;

        var phoneExist = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.Phone == registerCustomer.Phone && e.User.IsActive);
        if (phoneExist is not null) return 0;

        var usernameExist = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.Username == registerCustomer.Username && e.IsActive);
        if (usernameExist is not null) return 0;

        var user = await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = registerCustomer.Email,
            Role = UserRole.Customer,
            Username = registerCustomer.Username,
            IsActive = true,
            PasswordHash = _passwordHasher.HashPassword(registerCustomer.Password)
        });

        await _context.Customers.AddAsync(new Customer
        {
            Id = Guid.NewGuid(),
            Name = registerCustomer.Name,
            Phone = registerCustomer.Phone,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UserId = user.Entity.Id
        });

        return await _context.SaveChangesAsync();
    }

    public async Task<int> RegisterSeller(RegisterSeller registerSeller)
    {
        var userExist = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.Email == registerSeller.Email && e.IsActive);
        if (userExist is not null) return 0;

        var usernameExist = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.Username == registerSeller.Username);
        if (usernameExist is not null) return 0;

        var phoneExist = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => 
                e.Phone == registerSeller.Phone);
        if (phoneExist is not null) return 0;

        var user = await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = registerSeller.Email,
            Role = UserRole.Seller,
            Username = registerSeller.Username,
            IsActive = true,
            PasswordHash = _passwordHasher.HashPassword(registerSeller.Password)
        });

        await _context.Sellers.AddAsync(new Seller
        {
            Id = Guid.NewGuid(),
            Name = registerSeller.Name,
            Phone = registerSeller.Phone,
            StoreName = registerSeller.StoreName,
            UserId = user.Entity.Id
        });

        return await _context.SaveChangesAsync();
    }

    public async Task<string> Login(LoginUser loginUser)
    {
        var user = await _context.Users
            .Where(e => e.Username == loginUser.Username && e.IsActive)
            .Select(u => new
            {
                u.PasswordHash,
                u.Role,
                CustomerId = u.Role == UserRole.Customer ? u.Customer.Id : (Guid?)null,
                SellerId = u.Role == UserRole.Seller ? u.Seller.Id : (Guid?)null
            })
            .FirstOrDefaultAsync();

        if (user is null || !_passwordHasher.VerifyPassword(loginUser.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var userId = user.Role == UserRole.Customer ? user.CustomerId!.Value : user.SellerId!.Value;

        return _jwtService.GenerateToken(userId, user.Role);
    }
}