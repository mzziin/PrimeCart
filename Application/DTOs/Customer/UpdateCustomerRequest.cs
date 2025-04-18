namespace Application.DTOs.Customer;

public class UpdateCustomerRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}