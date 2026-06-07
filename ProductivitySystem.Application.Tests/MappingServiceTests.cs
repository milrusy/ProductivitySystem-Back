using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.UnitTests.Application;

public class MappingServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly MappingService _service;
    private readonly List<ExternalUserMapping> _mappingsDb;

    public MappingServiceTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);

        _mappingsDb = new List<ExternalUserMapping>();
        _service = new MappingService(_contextMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedObjectsWithUserNames()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Danylo" };
        _mappingsDb.Add(new ExternalUserMapping { Id = 10, UserId = 1, GitHubLogin = "danylo-gh", User = user });

        var mockSet = _mappingsDb.BuildMockDbSet();
        _contextMock.Setup(c => c.ExternalUserMappings).Returns(mockSet.Object);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().ContainSingle();
        result.First().ToString().Should().Contain("UserName = Danylo");
        result.First().ToString().Should().Contain("GitHubLogin = danylo-gh");
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var mockSet = _mappingsDb.BuildMockDbSet();
        _contextMock.Setup(c => c.ExternalUserMappings).Returns(mockSet.Object);

        // Act & Assert
        Func<Task> act = async () => await _service.DeleteAsync(99);
        await act.Should().ThrowAsync<Exception>().WithMessage("Mapping not found");
    }
}
