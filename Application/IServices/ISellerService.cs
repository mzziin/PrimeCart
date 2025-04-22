using Application.Common;
using Application.DTOs.Seller;

namespace Application.IServices;

public interface ISellerService
{
    Task<Result<SellerResponse>> GetSellerById(Guid id);
    Task<Result<SellerResponse>> UpdateSeller(Guid id, UpdateSellerRequest? updateSellerRequest);
    Task<Result<bool>> DeleteSeller(Guid id);
}