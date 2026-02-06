using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.DTOs;

public class PhoneDto
{
    [Required]
    public required string PhoneNumber { get; set; }
    public string? Type { get; set; }
}
