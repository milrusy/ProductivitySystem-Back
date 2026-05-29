using Moq;
using FluentAssertions;
using MockQueryable.Moq;using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.UnitTests.Application;

public class UserAnalyticsServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly UserAnalyticsService _service;

    public UserAnalyticsServiceTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);
        _service = new UserAnalyticsService(_contextMock.Object);
    }

    [Fact]
    public async Task GetEmployeeDetails_WithValidId_ShouldReturnFullAggregatedDto()
    {
        // Arrange
        var userId = 5;
        var users = new List<User>
        {
            new() { Id = userId, Name = "Alice", Email = "alice@corp.com", Role = "Employee", Department = new Department { Name = "QA" } }
        }.BuildMockDbSet();

        var metrics = new List<Metric>
        {
            new() { UserId = userId, CompletedTasks = 12, OverdueTasks = 1, AvgCompletionTime = 6.2, ProductivityScore = 91.0 }
        }.BuildMockDbSet();

        var tasks = new List<ExternalTask>
        {
            new() { AssigneeId = userId, Title = "Write test cases", Status = "Done", Priority = "High", Deadline = DateTime.UtcNow }
        }.BuildMockDbSet();

        _contextMock.Setup(c => c.Users).Returns(users.Object);
        _contextMock.Setup(c => c.Metrics).Returns(metrics.Object);
        _contextMock.Setup(c => c.Tasks).Returns(tasks.Object);

        // Act
        var result = await _service.GetEmployeeDetails(userId);

        // Assert
        result.Name.Should().Be("Alice");
        result.Department.Should().Be("QA");
        result.ProductivityScore.Should().Be(91.0);
        result.Tasks.Should().ContainSingle();
        result.Tasks.First().Title.Should().Be("Write test cases");
    }

    [Fact]
    public async Task GetEmployeeDetails_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var users = new List<User>().BuildMockDbSet();
        _contextMock.Setup(c => c.Users).Returns(users.Object);

        // Act & Assert
        Func<Task> act = async () => await _service.GetEmployeeDetails(999);
        await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
    }
}
