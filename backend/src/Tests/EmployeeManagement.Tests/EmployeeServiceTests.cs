using Xunit;
using Moq;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace EmployeeManagement.Tests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<EmployeeService>> _loggerMock;
    private readonly EmployeeService _sut; // System Under Test

    public EmployeeServiceTests()
    {
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<EmployeeService>>();

        _sut = new EmployeeService(
            _employeeRepositoryMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldCreateEmployee_WhenDataIsValid()
    {
        // Arrange
        var createDto = new EmployeeCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            DocumentNumber = "12345678900",
            Password = "Password123",
            DateOfBirth = DateTime.UtcNow.AddYears(-20), // 20 years old
            Role = EmployeeRole.Employee,
            Phones = new List<PhoneDto>()
        };

        _employeeRepositoryMock.Setup(x => x.ExistsAsync(createDto.Email)).ReturnsAsync(false);
        _authServiceMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed_password");

        // Act
        var result = await _sut.CreateEmployeeAsync(createDto, 1, EmployeeRole.Director);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Email, result.Email);
        _employeeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldThrowArgumentException_WhenUnderage()
    {
        // Arrange
        var createDto = new EmployeeCreateDto
        {
            FirstName = "Kid",
            LastName = "Doe",
            Email = "kid.doe@test.com",
            DocumentNumber = "12345678900",
            Password = "Password123",
            DateOfBirth = DateTime.UtcNow.AddYears(-17), // 17 years old
            Role = EmployeeRole.Employee,
             Phones = new List<PhoneDto>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _sut.CreateEmployeeAsync(createDto, 1, EmployeeRole.Director));
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldThrowUnauthorizedAccessException_WhenRoleIsHigherThanRequester()
    {
        // Arrange
        var createDto = new EmployeeCreateDto
        {
            FirstName = "Future Boss",
            LastName = "Doe",
            Email = "boss.doe@test.com",
             DocumentNumber = "12345678900",
            Password = "Password123",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Role = EmployeeRole.Director, // Higher than requester
             Phones = new List<PhoneDto>()
        };

        // Act & Assert
        // Requester is Leader (2), trying to create Director (3) -> Should fail if logic is Role > RequesterRole
        // Director > Leader.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _sut.CreateEmployeeAsync(createDto, 1, EmployeeRole.Leader)); 
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldThrowArgumentException_WhenEmailExists()
    {
        // Arrange
        var createDto = new EmployeeCreateDto
        {
            FirstName = "Duplicate",
            LastName = "User",
            Email = "exists@test.com",
             DocumentNumber = "12345678900",
            Password = "Password123",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Role = EmployeeRole.Employee,
             Phones = new List<PhoneDto>()
        };

        _employeeRepositoryMock.Setup(x => x.ExistsAsync(createDto.Email)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _sut.CreateEmployeeAsync(createDto, 1, EmployeeRole.Director));
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenExists()
    {
        // Arrange
        var employee = new Employee 
        { 
            Id = 1, 
            FirstName = "Test", 
            LastName = "User", 
            Email = "test@test.com", 
            DocumentNumber="123", 
            PasswordHash="hash",
            DateOfBirth = DateTime.UtcNow.AddYears(-25)
        };
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);

        // Act
        var result = await _sut.GetEmployeeByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }
    
    [Fact]
    public async Task DeleteEmployeeAsync_ShouldReturnTrue_WhenEmployeeExists()
    {
         // Arrange
        var employee = new Employee 
        { 
            Id = 1, 
            FirstName = "Test", 
            LastName = "User", 
            Email = "test@test.com", 
            DocumentNumber="123", 
            PasswordHash="hash",
            DateOfBirth = DateTime.UtcNow.AddYears(-25)
        };
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);

        // Act
        var result = await _sut.DeleteEmployeeAsync(1);

        // Assert
        Assert.True(result);
        _employeeRepositoryMock.Verify(x => x.DeleteAsync(employee), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldReturnFalse_WhenEmployeeDoesNotExist()
    {
         // Arrange
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Employee?)null);

        // Act
        var result = await _sut.DeleteEmployeeAsync(1);

        // Assert
        Assert.False(result);
        _employeeRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Employee>()), Times.Never);
    }
}
