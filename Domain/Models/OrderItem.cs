using Domain.Enums;

namespace Domain.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateOnly DeliveredAt { get; set; }
    
    public Product Product { get; set; }
    public Order Order { get; set; }
}