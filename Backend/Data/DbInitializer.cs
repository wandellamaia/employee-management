using EmployeeManagement.Models;
using EmployeeManagement.Services;

namespace EmployeeManagement.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context, IAuthService authService)
    {
        // Check if database created
        // context.Database.EnsureCreated(); // We use migrations instead

        // Look for any employees.
        if (context.Employees.Any())
        {
            return;   // DB has been seeded
        }

        var director = new Employee
        {
            FirstName = "Admin",
            LastName = "Director",
            Email = "admin@company.com",
            DocumentNumber = "00000000000",
            PasswordHash = authService.HashPassword("Admin123!"),
            Role = EmployeeRole.Director, // Level 3
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ManagerId = null
        };

        context.Employees.Add(director);
        context.SaveChanges();
    }
}
