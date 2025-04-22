namespace Application.DTOs.Seller;

public class UpdateSellerRequest
{
    public string? Name { get; set; }
    public string? StoreName { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}