using System.Security.AccessControl;

namespace Domain.Models;

public class Customer
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public DateOnly CreatedAt { get; set; }
    public Guid UserId { get; set; }

    public User User { get; set; }
    public Wishlist Wishlist { get; set; }
    public ShoppingCart ShoppingCart { get; set; }
    public ICollection<Address> Addresses { get; set; }
    public ICollection<Order> Orders { get; set; }
}