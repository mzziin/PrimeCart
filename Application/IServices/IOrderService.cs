using Application.Common;
using Application.DTOs.Order;
using Domain.Enums;

namespace Application.IServices;

public interface IOrderService
{
    Task<Result<List<OrderResponse>>> GetOrdersByCustomerId(Guid customerId);
    Task<Result<OrderResponse>> GetOrderById(Guid orderId);
    Task<Result<OrderResponse>> PlaceOrder(Guid customerId, PlaceOrderRequest placeOrderRequest);
    Task<Result<OrderItemResponse>> UpdateOrderStatus(Guid orderId, OrderItemStatus orderItemStatus);
    Task<Result<bool>> CancelOrder(Guid orderId);
}