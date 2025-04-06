namespace Domain.Models;

public class ShoppingCart
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    
    public Customer Customer { get; set; }
    public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
}