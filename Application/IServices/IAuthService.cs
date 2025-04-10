using Application.DTOs;

namespace Application.IServices;

public interface IAuthService
{
    void RegisterCustomer(RegisterCustomer registerCustomer);
    void RegisterSeller(RegisterSeller registerSeller);
    void Login(LoginUser loginUser);
    void Logout();
}