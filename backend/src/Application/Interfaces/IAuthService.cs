using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Interfaces;

public interface IAuthService
{
    Task<TokenDto?> LoginAsync(LoginDto loginDto);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
