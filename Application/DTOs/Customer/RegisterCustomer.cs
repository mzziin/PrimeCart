using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RegisterCustomer
{
    [Required]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
    [MaxLength(50, ErrorMessage = "Name can't exceed 50 characters.")]
    public required string Name { get; set; }
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(100, ErrorMessage = "Email can't exceed 100 characters.")]
    public required string Email { get; set; }
    
    [Required]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    [MaxLength(50, ErrorMessage = "Username can't exceed 50 characters.")]
    public required string Username { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(100, ErrorMessage = "Password can't exceed 100 characters.")]
    public required string Password { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;
    
    [Required]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 digits.")]
    public required string Phone { get; set; }
}