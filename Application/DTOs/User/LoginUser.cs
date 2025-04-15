using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class LoginUser
{
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
}