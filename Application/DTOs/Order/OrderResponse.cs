using Domain.Enums;

namespace Application.DTOs.Order;

public class OrderResponse
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    
    public ICollection<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public OrderItemStatus Status { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateOnly? DeliveredAt { get; set; }
}