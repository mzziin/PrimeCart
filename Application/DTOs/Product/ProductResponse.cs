namespace Application.DTOs.Product;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public DateOnly CreatedAt { get; set; }
}