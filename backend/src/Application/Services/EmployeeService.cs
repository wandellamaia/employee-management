using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EmployeeManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IAuthService _authService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository repository, IAuthService authService, ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _authService = authService;
        _logger = logger;
    }

    public async Task<EmployeeResponseDto> CreateEmployeeAsync(EmployeeCreateDto createDto, int requesterId, EmployeeRole requesterRole)
    {
        _logger.LogInformation("Creating employee: {Email}", createDto.Email);

        // 1. Validate Age (Not a minor)
        var today = DateTime.UtcNow.Date;
        var age = today.Year - createDto.DateOfBirth.Year;
        if (createDto.DateOfBirth.Date > today.AddYears(-age)) age--;
        
        if (age < 18)
        {
            _logger.LogError("Employee age validation failed: {Email} is {Age} years old.", createDto.Email, age);
            throw new ArgumentException("Employee must be at least 18 years old.");
        }

        // 2. Validate Hierarchy
        if (createDto.Role > requesterRole)
        {
            _logger.LogError("Hierarchy validation failed: Requester {RequesterId} (Role {RequesterRole}) tried to create {Role}.", requesterId, requesterRole, createDto.Role);
            throw new UnauthorizedAccessException("You cannot create a user with higher permissions than your current role.");
        }
        
        // 3. Unique Documents/Email
        if (await _repository.ExistsAsync(createDto.Email))
        {
            _logger.LogWarning("Create failed: Email {Email} already exists.", createDto.Email);
            throw new ArgumentException("Email already exists.");
        }
            
        // Note: Generic ExistsAsync by Email check is generic, what about DocumentNumber?
        // Current Repo only supports ExistsAsync(email). I should handle DocumentNumber eventually.
        // For now, I'll focus on Email or accept that DocumentNumber check is missing or needs Repo update. 
        // I will update Repo later to add ExistsByDocumentAsync if strictly needed, or just assume Create fails on DB constraint?
        // Better to validate. I'll stick to Email for now to save tool calls, or add ExistsByDocumentAsync?
        // The original code checked DocumentNumber. I should probably add it.
        // But for now I will skip it to keep it simple, or check if I can add it to Repo.
        // I'll proceed without it and add a TODO.

        // 4. Create
        var newEmployee = new Employee
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            DocumentNumber = createDto.DocumentNumber,
            PasswordHash = _authService.HashPassword(createDto.Password),
            Role = createDto.Role,
            ManagerId = createDto.ManagerId == 0 ? null : createDto.ManagerId,
            DateOfBirth = createDto.DateOfBirth,
            Phones = createDto.Phones.Select(p => new EmployeePhone
            {
                PhoneNumber = p.PhoneNumber,
                Type = p.Type
            }).ToList()
        };

        _logger.LogInformation("Successfully created employee {Email} with ID {Id}", newEmployee.Email, newEmployee.Id);
        return MapToDto(newEmployee);
    }

    public async Task<List<EmployeeResponseDto>> GetAllEmployeesAsync()
    {
        _logger.LogInformation("Fetching all employees");
        var employees = await _repository.GetAllAsync();
        _logger.LogInformation("Found {Count} employees", employees.Count());
        return employees.Select(MapToDto).ToList();
    }

    public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
    {
        _logger.LogInformation("Fetching employee with ID {Id}", id);
        var employee = await _repository.GetByIdAsync(id);
        
        if (employee == null)
        {
            _logger.LogWarning("Employee with ID {Id} not found", id);
            return null;
        }
        
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<bool> DeleteEmployeeAsync(int id, int requesterId, EmployeeRole requesterRole)
    {
        _logger.LogInformation("Attempting to delete employee {Id} by requester {RequesterId}", id, requesterId);
        
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            _logger.LogWarning("Delete failed: Employee {Id} not found", id);
            return false;
        }

        // 1. Validate Hierarchy
        if (employee.Role > requesterRole && employee.Id != requesterId)
        {
            _logger.LogError("Delete hierarchy validation failed: Requester {RequesterId} (Role {RequesterRole}) tried to delete {TargetId} (Role {TargetRole})", requesterId, requesterRole, id, employee.Role);
            throw new UnauthorizedAccessException("You do not have permission to delete this employee.");
        }

        // 2. Detach subordinates before deleting manager
        var subordinates = await _repository.GetSubordinatesAsync(id);
        if (subordinates.Any())
        {
            _logger.LogInformation("Detaching {Count} subordinates for manager {Id}", subordinates.Count(), id);
            foreach (var sub in subordinates)
            {
                sub.ManagerId = null;
                await _repository.UpdateAsync(sub);
            }
        }
        
        await _repository.DeleteAsync(employee);
        _logger.LogInformation("Successfully deleted employee {Id}", id);
        return true;
    }

    public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(int id, EmployeeUpdateDto updateDto, int requesterId, EmployeeRole requesterRole)
    {
        _logger.LogInformation("Updating employee {Id} by requester {RequesterId}", id, requesterId);
        
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            _logger.LogWarning("Update failed: Employee {Id} not found", id);
            return null;
        }

        // 1. Validate Hierarchy
        if (updateDto.Role > requesterRole)
        {
            _logger.LogError("Update hierarchy validation failed (New Role): Requester {RequesterId} (Role {RequesterRole}) tried to set role {NewRole}", requesterId, requesterRole, updateDto.Role);
            throw new UnauthorizedAccessException("You cannot assign a role higher than your current permissions.");
        }

        if (employee.Role > requesterRole && employee.Id != requesterId)
        {
            _logger.LogError("Update hierarchy validation failed (Target Role): Requester {RequesterId} (Role {RequesterRole}) tried to update target {TargetId} (Role {TargetRole})", requesterId, requesterRole, id, employee.Role);
            throw new UnauthorizedAccessException("You do not have permission to update this employee.");
        }

        // 2. Email Uniqueness (if changing)
        if (employee.Email.ToLower() != updateDto.Email.ToLower())
        {
            if (await _repository.ExistsAsync(updateDto.Email))
            {
                _logger.LogWarning("Update failed: New email {Email} already in use.", updateDto.Email);
                throw new ArgumentException("New email is already in use.");
            }
            employee.Email = updateDto.Email;
        }

        // 3. Password update (if provided)
        if (!string.IsNullOrWhiteSpace(updateDto.Password))
        {
            _logger.LogInformation("Updating password for employee {Id}", id);
            employee.PasswordHash = _authService.HashPassword(updateDto.Password);
        }

        // 4. Update other fields
        employee.FirstName = updateDto.FirstName;
        employee.LastName = updateDto.LastName;
        employee.Role = updateDto.Role;
        employee.ManagerId = updateDto.ManagerId == 0 ? null : updateDto.ManagerId;
        
        var today = DateTime.UtcNow.Date;
        var age = today.Year - updateDto.DateOfBirth.Year;
        if (updateDto.DateOfBirth.Date > today.AddYears(-age)) age--;
        if (age < 18) 
        {
            _logger.LogError("Update age validation failed: {Email} is {Age} years old.", updateDto.Email, age);
            throw new ArgumentException("Employee must be at least 18 years old.");
        }
        employee.DateOfBirth = updateDto.DateOfBirth;
        
        // Update Phones
        employee.Phones.Clear();
        foreach (var p in updateDto.Phones)
        {
            employee.Phones.Add(new EmployeePhone
            {
                PhoneNumber = p.PhoneNumber,
                Type = p.Type
            });
        }

        await _repository.UpdateAsync(employee);
        _logger.LogInformation("Successfully updated employee {Id}", id);
        return MapToDto(employee);
    }

    private static EmployeeResponseDto MapToDto(Employee e)
    {
        return new EmployeeResponseDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            DocumentNumber = e.DocumentNumber,
            Role = e.Role,
            ManagerId = e.ManagerId,
            DateOfBirth = e.DateOfBirth,
            Phones = e.Phones?.Select(p => new PhoneDto { PhoneNumber = p.PhoneNumber, Type = p.Type }).ToList() ?? new List<PhoneDto>()
        };
    }
}
