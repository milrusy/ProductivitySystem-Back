using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Tests;

public class MetricsServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly MetricsService _service;

    public MetricsServiceTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);
        _service = new MetricsService(_contextMock.Object);
    }

    [Fact]
    public async Task GetUserMetrics_ShouldApplyDateFiltersAndCalculateCorrectScore()
    {
        // Arrange
        var userId = 1;
        var tasks = new List<ExternalTask>
        {
            new() { Id = 1, AssigneeId = userId, Status = "Done", CreatedAt = new DateTime(2026, 05, 10), CompletedAt = new DateTime(2026, 05, 12), Deadline = new DateTime(2026, 05, 15) },
            new() { Id = 2, AssigneeId = userId, Status = "InProgress", CreatedAt = new DateTime(2026, 05, 11), CompletedAt = null, Deadline = new DateTime(2026, 05, 14) },
            new() { Id = 3, AssigneeId = userId, Status = "Done", CreatedAt = new DateTime(2026, 01, 01), CompletedAt = new DateTime(2026, 01, 02) }
        };

        var tasksMock = tasks.BuildMockDbSet();
        _contextMock.Setup(c => c.Tasks).Returns(tasksMock.Object);

        var fromDate = new DateTime(2026, 05, 01);
        var toDate = new DateTime(2026, 05, 20);

        // Act
        var result = await _service.GetUserMetrics(userId, fromDate, toDate);

        // Assert
        result.CompletedTasks.Should().Be(1);
        result.OverdueTasks.Should().Be(2);
        result.AvgCompletionTime.Should().Be(48.0);
        result.ProductivityScore.Should().Be(0.3);
    }

    [Fact]
    public async Task GetTrends_ShouldGroupCorrectlyAndApplyPenaltyForLateTasks()
    {
        // Arrange
        var completionDate = new DateTime(2026, 05, 25);
        var tasks = new List<ExternalTask>
        {
            new() { Id = 1, CompletedAt = completionDate, Deadline = completionDate.AddDays(1), Assignee = new User { DepartmentId = 1 } },
            new() { Id = 2, CompletedAt = completionDate, Deadline = completionDate.AddDays(-1), Assignee = new User { DepartmentId = 1 } }
        };

        var tasksMock = tasks.BuildMockDbSet();
        _contextMock.Setup(c => c.Tasks).Returns(tasksMock.Object);

        // Act
        var result = await _service.GetTrends(null, 1, null, null);

        // Assert
        result.Should().ContainSingle();
        result.First().Date.Should().Be("2026-05-25");
        result.First().Score.Should().Be(0.75);
    }
}
