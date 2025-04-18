using Application.Common;
using Application.DTOs.Customer;

namespace Application.IServices;

public interface ICustomerService
{
    Task<Result<CustomerResponse>> GetCustomerById(Guid id);
    Task<Result<CustomerResponse>> UpdateCustomer(Guid id, UpdateCustomerRequest? updateCustomerRequest);
    Task<Result<bool>> DeleteCustomer(Guid id);
}