using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeResponseDto> CreateEmployeeAsync(EmployeeCreateDto createDto, int requesterId, EmployeeRole requesterRole);
    Task<List<EmployeeResponseDto>> GetAllEmployeesAsync();
    Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id);
    Task<bool> DeleteEmployeeAsync(int id, int requesterId, EmployeeRole requesterRole);
    Task<EmployeeResponseDto?> UpdateEmployeeAsync(int id, EmployeeCreateDto updateDto, int requesterId, EmployeeRole requesterRole);
}
