using Application.Common;
using Domain.Enums;

namespace Application.IServices;

public interface IOrderService
{
    Task<Result<bool>> GetOrdersByCustomerId(Guid customerId);
    Task<Result<bool>> GetOrderById(Guid orderId);
    Task<Result<bool>> PlaceOrder(Guid customerId);
    Task<Result<bool>> UpdateOrderStatus(Guid orderId, OrderItemStatus orderItemStatus);
    Task<Result<bool>> CancelOrder(Guid orderId);
}