namespace Domain.Models;

public class ShoppingCartItem
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    
    public ShoppingCart Cart { get; set; }
    public Product Product { get; set; }
}