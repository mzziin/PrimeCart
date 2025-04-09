namespace Domain.Models;

public class Seller
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string StoreName { get; set; }
    public string Phone { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    public ICollection<Product> Products { get; set; } = new List<Product>();
}