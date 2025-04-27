namespace Application.DTOs.Wishlist;

public class WishlistResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    
    public ICollection<WishlistItemResponse> WishlistItems { get; set; } = new List<WishlistItemResponse>();
}

public class WishlistItemResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
}