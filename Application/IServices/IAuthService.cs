using Application.DTOs;

namespace Application.IServices;

public interface IAuthService
{
    Task<int> RegisterCustomer(RegisterCustomer registerCustomer);
    Task<int> RegisterSeller(RegisterSeller registerSeller);
    Task<string> Login(LoginUser loginUser);
}