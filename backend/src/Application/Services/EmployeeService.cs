using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Attempting to create employee with email: {Email}", createDto.Email);

        // 1. Validate Age (Not a minor)
        var today = DateTime.UtcNow.Date;
        var age = today.Year - createDto.DateOfBirth.Year;
        if (createDto.DateOfBirth.Date > today.AddYears(-age)) age--;
        
        if (age < 18)
        {
            _logger.LogWarning("Creation failed: Employee {Email} is a minor ({Age} years old).", createDto.Email, age);
            throw new ArgumentException("Employee must be at least 18 years old.");
        }

        // 2. Validate Hierarchy
        if (createDto.Role > requesterRole)
        {
             _logger.LogWarning("Creation failed: User {RequesterId} (Role: {RequesterRole}) tried to create higher privilege user (Role: {NewRole}).", requesterId, requesterRole, createDto.Role);
             throw new UnauthorizedAccessException("You cannot create a user with higher permissions than your current role.");
        }
        
        // 3. Unique Documents/Email
        if (await _repository.ExistsAsync(createDto.Email))
        {
            _logger.LogWarning("Creation failed: Email {Email} already exists.", createDto.Email);
            throw new ArgumentException("Email already exists.");
        }

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

        await _repository.AddAsync(newEmployee);
        _logger.LogInformation("Employee {Id} created successfully.", newEmployee.Id);

        return MapToDto(newEmployee);
    }

    public async Task<List<EmployeeResponseDto>> GetAllEmployeesAsync()
    {
        var employees = await _repository.GetAllAsync();
        return employees.Select(MapToDto).ToList();
    }

    public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) _logger.LogWarning("Employee {Id} not found.", id);
        
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<bool> DeleteEmployeeAsync(int id, int requesterId, EmployeeRole requesterRole)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return false;

        // 1. Validate Hierarchy
        // A user can delete themselves or anyone with a role <= their own
        if (employee.Role > requesterRole && employee.Id != requesterId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this employee.");
        }

        // 2. Detach subordinates before deleting manager
        var subordinates = await _repository.GetSubordinatesAsync(id);
        foreach (var sub in subordinates)
        {
            sub.ManagerId = null;
            await _repository.UpdateAsync(sub);
        }
        
        await _repository.DeleteAsync(employee);
        _logger.LogInformation("Employee {Id} deleted successfully.", id);
        return true;
    }

    public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(int id, EmployeeCreateDto updateDto, int requesterId, EmployeeRole requesterRole)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
             _logger.LogWarning("Update failed: Employee {Id} not found.", id);
             return null;
        }

        // 1. Validate Hierarchy
        if (updateDto.Role > requesterRole)
            throw new UnauthorizedAccessException("You cannot assign a role higher than your current permissions.");

        if (employee.Role > requesterRole && employee.Id != requesterId)
            throw new UnauthorizedAccessException("You do not have permission to update this employee.");

        // 2. Email Uniqueness (if changing)
        if (employee.Email.ToLower() != updateDto.Email.ToLower())
        {
            if (await _repository.ExistsAsync(updateDto.Email))
                throw new ArgumentException("New email is already in use.");
            employee.Email = updateDto.Email;
        }

        // 3. Password update (if provided)
        if (!string.IsNullOrWhiteSpace(updateDto.Password))
        {
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
            _logger.LogWarning("Update failed: Employee becomes minor ({Age} years old).", age);
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
        _logger.LogInformation("Employee {Id} updated successfully.", id);
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
