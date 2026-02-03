using EmployeeManagement.Data;
using EmployeeManagement.DTOs;
using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeManagement.Services;

public interface IAuthService
{
    Task<TokenDto?> LoginAsync(LoginDto loginDto);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<TokenDto?> LoginAsync(LoginDto loginDto)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == loginDto.Email);

        if (employee == null || !VerifyPassword(loginDto.Password, employee.PasswordHash))
        {
            return null;
        }

        var token = GenerateJwtToken(employee);
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
