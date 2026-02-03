using EmployeeManagement.DTOs;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var token = await _authService.LoginAsync(loginDto);
        if (token == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(token);
    }
}
