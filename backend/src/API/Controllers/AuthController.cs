using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("POST login request for: {Email}", loginDto.Email);
        var token = await _authService.LoginAsync(loginDto);
        if (token == null)
        {
            _logger.LogWarning("Invalid login attempt for: {Email}", loginDto.Email);
            return Unauthorized("Invalid email or password.");
        }

        _logger.LogInformation("Login successful for: {Email}", loginDto.Email);
        return Ok(token);
    }
}
