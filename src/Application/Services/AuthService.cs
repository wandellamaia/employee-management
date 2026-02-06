using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IEmployeeRepository employeeRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _employeeRepository = employeeRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenDto?> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);
        var employee = await _employeeRepository.GetByEmailAsync(loginDto.Email);

        if (employee == null || !VerifyPassword(loginDto.Password, employee.PasswordHash))
        {
            _logger.LogWarning("Login failed for email: {Email}. Invalid credentials.", loginDto.Email);
            return null;
        }

        var token = GenerateJwtToken(employee);
        _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
        return new TokenDto 
        { 
            Token = token, 
            Role = employee.Role.ToString(),
            Expiration = DateTime.UtcNow.AddHours(8)
        };
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string GenerateJwtToken(Employee employee)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, employee.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", employee.Role.ToString()),
            new Claim("Id", employee.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
