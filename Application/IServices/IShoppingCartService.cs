using Application.Common;
using Application.DTOs.ShoppingCart;

namespace Application.IServices;

public interface IShoppingCartService
{
    Task<Result<ShoppingCartResponse>> GetCart(Guid customerId);
    Task<Result<ShoppingCartResponse>> AddToCart(Guid customerId, Guid productId, int quantity);
    Task<Result<ShoppingCartResponse>> UpdateCart(Guid customerId, Guid productId, int quantity);
    Task<Result<ShoppingCartResponse>> RemoveFromCart(Guid customerId, Guid productId);
    Task<Result<bool>> ClearCart(Guid customerId);
}