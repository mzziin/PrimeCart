using Application.Common;
using Application.DTOs.Wishlist;

namespace Application.IServices;

public interface IWishlistService
{
    Task<Result<WishlistResponse>> GetWishlist(Guid customerId);
    Task<Result<WishlistResponse>> AddToWishlist(Guid customerId, Guid productId);
    Task<Result<WishlistResponse>> RemoveFromWishlist(Guid customerId, Guid productId);
    Task<Result<bool>> ClearWishlist(Guid customerId);
}