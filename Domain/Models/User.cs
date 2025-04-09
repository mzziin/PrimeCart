using Domain.Enums;

namespace Domain.Models;

public class User
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public Customer Customer { get; set; }
    public Seller Seller { get; set; }
}