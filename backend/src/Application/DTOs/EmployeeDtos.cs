using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.DTOs;

public class EmployeeCreateDto
{
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
    [Required]
    public required string DocumentNumber { get; set; }
    
    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
    
    [Required]
    public EmployeeRole Role { get; set; }
    
    public int? ManagerId { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    public List<PhoneDto> Phones { get; set; } = new();
}
