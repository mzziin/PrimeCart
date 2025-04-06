namespace Domain.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public DateOnly CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public Seller Seller { get; set; }
    public Category Category { get; set; }
}