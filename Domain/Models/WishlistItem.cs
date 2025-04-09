namespace Domain.Models;

public class WishlistItem
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public Guid WishlistId { get; set; }
    public Guid ProductId { get; set; }
    
    public Wishlist Wishlist { get; set; }
    public Product Product { get; set; }
}