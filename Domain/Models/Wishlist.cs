namespace Domain.Models;

public class Wishlist
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public Guid CustomerId { get; set; }
    
    public Customer Customer { get; set; }
    public ICollection<WishlistItem> WishlistItems { get; set; }
}