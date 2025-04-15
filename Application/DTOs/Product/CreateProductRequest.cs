namespace Application.DTOs.Product;

public class CreateProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; } 
    public Guid CategoryId { get; set; }
    public Guid SellerId { get; set; } 
}