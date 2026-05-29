using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using Microsoft.AspNetCore.Identity;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.UnitTests.Application;

public class UserManagementServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly UserManagementService _service;
    private readonly List<User> _usersDb;

    public UserManagementServiceTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);

        _usersDb = new List<User>();
        _service = new UserManagementService(_contextMock.Object);
    }

    [Fact]
    public async Task CreateUser_ShouldSaveHashedPassword_WhenEmailIsUnique()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "New Guy", Email = "new@corp.com", DepartmentId = 1, Role = "Employee" };
        var usersMock = _usersDb.BuildMockDbSet();

        usersMock.Setup(m => m.Add(It.IsAny<User>())).Callback<User>(_usersDb.Add);
        _contextMock.Setup(c => c.Users).Returns(usersMock.Object);

        // Act
        var temporaryPassword = await _service.CreateUser(dto);

        // Assert
        temporaryPassword.Should().HaveLength(10); // Слайс [..10]
        _usersDb.Should().ContainSingle();
        _usersDb.First().Email.Should().Be("new@corp.com");

        _usersDb.First().PasswordHash.Should().NotBe(temporaryPassword);

        var hasher = new PasswordHasher<User>();
        var verifyResult = hasher.VerifyHashedPassword(_usersDb.First(), _usersDb.First().PasswordHash, temporaryPassword);
        verifyResult.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task ChangePassword_ShouldUpdateHash_WhenOldPasswordIsCorrect()
    {
        // Arrange
        var hasher = new PasswordHasher<User>();
        var targetUser = new User { Id = 1, Email = "user@corp.com" };
        targetUser.PasswordHash = hasher.HashPassword(targetUser, "OldPassword123!");

        _usersDb.Add(targetUser);
        var usersMock = _usersDb.BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(usersMock.Object);

        var dto = new ChangePasswordDto { OldPassword = "OldPassword123!", NewPassword = "SecureNewPassword555!" };

        // Act
        await _service.ChangePassword(1, dto);

        // Assert
        var checkResult = hasher.VerifyHashedPassword(targetUser, targetUser.PasswordHash, "SecureNewPassword555!");
        checkResult.Should().Be(PasswordVerificationResult.Success);
    }
}
