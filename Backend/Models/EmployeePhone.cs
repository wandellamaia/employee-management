using System.Text.Json.Serialization;

namespace EmployeeManagement.Models;

public class EmployeePhone
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    
    [JsonIgnore]
    public Employee? Employee { get; set; }
    
    public required string PhoneNumber { get; set; }
    public string? Type { get; set; } // Mobile, Home, Work, etc.
}
