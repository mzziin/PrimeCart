using Application.DTOs.Order;
using Domain.Models;

namespace Infrastructure.Mappers;

public static class OrderMappers
{
    public static OrderItemResponse ToResponse(this OrderItem orderItem)
    {
        return new OrderItemResponse
        {
            Id = orderItem.Id,
            ProductId = orderItem.ProductId,
            ProductName = orderItem.Product.Name,
            Status = orderItem.Status,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.Price,
            TotalPrice = orderItem.Price * orderItem.Quantity,
            DeliveredAt = orderItem.DeliveredAt
        };
    }
}