using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateDto createDto)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<EmployeeRole>(roleClaim, out var requesterRole))
        {
            _logger.LogWarning("Create Employee failed: Invalid role claim.");
            return Unauthorized("Role claim missing or invalid.");
        }
        
        var idClaim = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(idClaim, out int requesterId);

        try
        {
            _logger.LogInformation("POST request to create employee by user {RequesterId}", requesterId);
            var result = await _employeeService.CreateEmployeeAsync(createDto, requesterId, requesterRole);
            _logger.LogInformation("Successfully created employee with ID {Id}", result.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in Create: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized error in Create: {Message}", ex.Message);
            return StatusCode(403, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GET all employees request");
        var result = await _employeeService.GetAllEmployeesAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("GET employee request for ID {Id}", id);
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) 
        {
            _logger.LogWarning("Employee {Id} not found", id);
            return NotFound();
        }
        return Ok(employee);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<EmployeeRole>(roleClaim, out var requesterRole))
        {
            return Unauthorized("Role claim missing or invalid.");
        }

        var idClaim = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(idClaim, out int requesterId);

        try
        {
            _logger.LogInformation("DELETE request for employee {Id} by user {RequesterId}", id, requesterId);
            var success = await _employeeService.DeleteEmployeeAsync(id, requesterId, requesterRole);
            if (!success)
            {
                _logger.LogWarning("Employee {Id} not found for deletion", id);
                return NotFound();
            }
            _logger.LogInformation("Successfully deleted employee {Id}", id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized error in Delete: {Message}", ex.Message);
            return StatusCode(403, ex.Message);
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateDto updateDto)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        if (string.IsNullOrEmpty(roleClaim) || !Enum.TryParse<EmployeeRole>(roleClaim, out var requesterRole))
        {
            return Unauthorized("Role claim missing or invalid.");
        }

        var idClaim = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(idClaim, out int requesterId);

        try
        {
            _logger.LogInformation("PUT request for employee {Id} by user {RequesterId}", id, requesterId);
            var result = await _employeeService.UpdateEmployeeAsync(id, updateDto, requesterId, requesterRole);
            if (result == null)
            {
                _logger.LogWarning("Employee {Id} not found for update", id);
                return NotFound();
            }
            _logger.LogInformation("Successfully updated employee {Id}", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in Update: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized error in Update: {Message}", ex.Message);
            return StatusCode(403, ex.Message);
        }
    }
}
