namespace Application.DTOs.ShoppingCart;

public class ShoppingCartResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    
    public ICollection<ShoppingCartItemResponse> ShoppingCartItems { get; set; } = new List<ShoppingCartItemResponse>();
}

public class ShoppingCartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}