using Application.Common;
using Application.DTOs.Customer;
using Application.IServices;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public CustomerService(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<CustomerResponse>> GetCustomerById(Guid id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.User.IsActive && c.Id == id)
            .Select(c => new CustomerResponse
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
            })
            .FirstOrDefaultAsync();

        if (customer is null)
            return Result<CustomerResponse>.Failure("Customer not found", 404);

        return Result<CustomerResponse>.Success(customer, 200);
    }

    public async Task<Result<CustomerResponse>> UpdateCustomer(Guid id, UpdateCustomerRequest? updateCustomerRequest)
    {
        if (updateCustomerRequest == null)
            return Result<CustomerResponse>.Failure("Update request cannot be null", 400);
        
        // Check if any fields to update were provided
        bool hasUpdates = !string.IsNullOrEmpty(updateCustomerRequest.Name) ||
                          !string.IsNullOrEmpty(updateCustomerRequest.Phone) ||
                          !string.IsNullOrEmpty(updateCustomerRequest.Password);

        if (!hasUpdates)
            return Result<CustomerResponse>.Failure("No update fields provided", 400);
        
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var customer = await _context.Customers
                .Where(c => c.User.IsActive)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer is null)
            {
                await transaction.RollbackAsync();
                return Result<CustomerResponse>.Failure("Customer not found", 404);
            }
            
            bool isModified = false;

            if (!string.IsNullOrEmpty(updateCustomerRequest.Name))
            {
                customer.Name = updateCustomerRequest.Name;
                isModified = true;
            }

            if (!string.IsNullOrEmpty(updateCustomerRequest.Password) &&
                !string.IsNullOrEmpty(updateCustomerRequest.ConfirmPassword) &&
                updateCustomerRequest.Password == updateCustomerRequest.ConfirmPassword)
            {
                var passwordHash = _passwordHasher.HashPassword(updateCustomerRequest.Password);
                customer.User.PasswordHash = passwordHash;
                isModified = true;
            }

            if (!string.IsNullOrEmpty(updateCustomerRequest.Phone) && updateCustomerRequest.Phone != customer.Phone)
            {
                var phoneExists = await _context.Customers
                    .Where(c => c.User.IsActive)
                    .Where(c => c.Id != id)
                    .AnyAsync(c => c.Phone == updateCustomerRequest.Phone);

                if (phoneExists)
                {
                    await transaction.RollbackAsync();
                    return Result<CustomerResponse>.Failure("Phone already exists", 409);
                }

                customer.Phone = updateCustomerRequest.Phone;
                isModified = true;
            }
            
            if (!isModified)
            {
                await transaction.CommitAsync(); // Nothing to rollback, but formally complete the transaction
                return Result<CustomerResponse>.Success(new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Phone = customer.Phone,
                    CreatedAt = customer.CreatedAt
                });
            }
            
            var result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                await transaction.RollbackAsync();
                return Result<CustomerResponse>.Failure("Failed to update customer");
            }
                
            await transaction.CommitAsync();

            var response = new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                CreatedAt = customer.CreatedAt
            };

            return Result<CustomerResponse>.Success(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<CustomerResponse>.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteCustomer(Guid id)
    {
        var customer = await _context.Customers
            .Where(c => c.User.IsActive && c.Id == id)
            .Include(customer => customer.User)
            .FirstOrDefaultAsync();

        if (customer is null)
            return Result<bool>.Failure("Customer not found", 404);

        customer.User.IsActive = false;
        var result = await _context.SaveChangesAsync();

        if (result == 0)
            return Result<bool>.Failure("Failed to delete customer");

        return Result<bool>.Success(true);
    }
}