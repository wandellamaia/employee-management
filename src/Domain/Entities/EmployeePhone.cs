namespace EmployeeManagement.Domain.Entities;

public class EmployeePhone
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    
    public Employee? Employee { get; set; }
    
    public required string PhoneNumber { get; set; }
    public string? Type { get; set; } // Mobile, Home, Work, etc.
}
