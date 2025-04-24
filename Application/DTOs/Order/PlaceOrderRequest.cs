using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public class PlaceOrderRequest
{
    [Required]
    public required ShippingAddressRequest ShippingAddress { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required to place an order")]
    public required ICollection<OrderItemRequest> OrderItems { get; set; }
}


public class OrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

}

public class ShippingAddressRequest
{
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Street { get; set; }
    [Required]
    public required string City { get; set; }
    [Required]
    public required string State { get; set; }
    [Required]
    public required string Country { get; set; }
}