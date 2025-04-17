using System.Net;
using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace PrimeCart.Controllers
{

    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("/customer/register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomer registerCustomer)
        {
            var result = await _authService.RegisterCustomer(registerCustomer);

            return StatusCode(result.StatusCode);
        }

        [HttpPost("/seller/register")]
        public async Task<IActionResult> RegisterSeller([FromBody] RegisterSeller registerSeller)
        {
            var result = await _authService.RegisterSeller(registerSeller);
            
            return StatusCode(result.StatusCode);
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
        {
            var result = await _authService.Login(loginUser);
            if (!result.IsSuccess) 
                return StatusCode(result.StatusCode);
            return StatusCode(result.StatusCode, result.Value);
        }
    }
}