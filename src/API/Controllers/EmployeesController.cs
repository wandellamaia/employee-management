using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            _logger.LogInformation("User {RequesterId} requesting to create a new employee.", requesterId);
            var result = await _employeeService.CreateEmployeeAsync(createDto, requesterId, requesterRole);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Create Employee failed: Argument exception.");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Create Employee failed: Unauthorized access.");
            return Forbid(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _employeeService.GetAllEmployeesAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var idClaim = User.FindFirst("Id")?.Value;
        _logger.LogInformation("User {RequesterId} requesting to delete employee {TargetId}.", idClaim, id);
        
        var success = await _employeeService.DeleteEmployeeAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeCreateDto updateDto)
    {
         try
        {
            var idClaim = User.FindFirst("Id")?.Value;
            _logger.LogInformation("User {RequesterId} requesting to update employee {TargetId}.", idClaim, id);

            var result = await _employeeService.UpdateEmployeeAsync(id, updateDto);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Update Employee failed: Argument exception.");
            return BadRequest(ex.Message);
        }
    }
}
