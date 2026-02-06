using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Phones)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    
    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _context.Employees
            .Include(e => e.Phones)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees
            .Include(e => e.Phones)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Employees.AnyAsync(e => e.Email.ToLower() == email.ToLower());
    }

    public async Task AddAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Employee employee)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Employee>> GetSubordinatesAsync(int managerId)
    {
        return await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .ToListAsync();
    }
}
