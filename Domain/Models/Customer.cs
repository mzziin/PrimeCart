namespace Domain.Models;

public class Customer
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public DateOnly CreatedAt { get; set; }

    public User User { get; set; }
    public Wishlist Wishlist { get; set; }
    public ShoppingCart ShoppingCart { get; set; }
    public ICollection<Address> Addresses { get; set; }
    public ICollection<Order> Orders { get; set; }
}