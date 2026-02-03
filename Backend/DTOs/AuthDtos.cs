using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
    [Required]
    public required string Password { get; set; }
}

public class TokenDto
{
    public required string Token { get; set; }
    public required string Role { get; set; }
    public DateTime Expiration { get; set; }
}
