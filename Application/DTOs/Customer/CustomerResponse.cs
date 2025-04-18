namespace Application.DTOs.Customer;

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public DateOnly CreatedAt { get; set; }
}