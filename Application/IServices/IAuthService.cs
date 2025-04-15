using Application.Common;
using Application.DTOs;

namespace Application.IServices;

public interface IAuthService
{
    Task<Result<bool>> RegisterCustomer(RegisterCustomer registerCustomer);
    Task<Result<bool>> RegisterSeller(RegisterSeller registerSeller);
    Task<Result<string>> Login(LoginUser loginUser);
}