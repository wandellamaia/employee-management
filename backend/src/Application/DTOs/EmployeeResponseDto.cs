using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.DTOs;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string DocumentNumber { get; set; }
    public EmployeeRole Role { get; set; }
    public int? ManagerId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<PhoneDto> Phones { get; set; } = new();
}
