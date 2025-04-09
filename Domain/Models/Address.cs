namespace Domain.Models;

public class Address
{
    public Guid Id { get; set; }
    public int RowId { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public Guid customerId { get; set; }
    
    public Customer customer { get; set; }
}