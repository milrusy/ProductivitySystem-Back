using Xunit;
using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;
using ProductivitySystem.Application.Services;

namespace ProductivitySystem.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly AuthService _service;
    private readonly PasswordHasher<User> _hasher;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);
        _jwtMock = new Mock<IJwtService>();
        _hasher = new PasswordHasher<User>();

        _service = new AuthService(_contextMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithWrongEmail_ShouldThrowException()
    {
        // Arrange
        var users = new List<User>().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        var request = new LoginRequestDto { Email = "fake@company.com", Password = "123" };

        // Act & Assert
        Func<Task> act = async () => await _service.LoginAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowException()
    {
        // Arrange
        var user = new User { Id = 1, Email = "dev@company.com", Name = "Developer" };
        user.PasswordHash = _hasher.HashPassword(user, "RealPassword123!");

        var usersMock = new List<User> { user }.BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(usersMock.Object);

        var request = new LoginRequestDto { Email = "dev@company.com", Password = "WrongPassword" };

        // Act & Assert
        Func<Task> act = async () => await _service.LoginAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("Invalid credentials");
    }
}