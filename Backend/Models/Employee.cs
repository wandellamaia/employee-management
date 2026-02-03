using System.Text.Json.Serialization;

namespace EmployeeManagement.Models;

public enum EmployeeRole
{
    Employee = 1,
    Leader = 2,
    Director = 3
}

public class Employee
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string DocumentNumber { get; set; }
    
    [JsonIgnore]
    public required string PasswordHash { get; set; }
    
    public EmployeeRole Role { get; set; }
    
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    public ICollection<EmployeePhone> Phones { get; set; } = new List<EmployeePhone>();
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
}
