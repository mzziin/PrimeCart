namespace Domain.Models;

public class Product
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public DateOnly CreatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    public Seller Seller { get; set; }
    public Category Category { get; set; }
    public ShoppingCartItem ShoppingCartItem { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<WishlistItem> WishlistItems { get; set; }
}