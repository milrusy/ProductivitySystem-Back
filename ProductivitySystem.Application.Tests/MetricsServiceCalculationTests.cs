using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.UnitTests.Application;

public class MetricsCalculationServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly MetricsCalculationService _service;
    private readonly List<Metric> _metricsDb;

    public MetricsCalculationServiceTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);

        _metricsDb = new List<Metric>();
        _service = new MetricsCalculationService(_contextMock.Object);
    }

    [Fact]
    public async Task CalculateMetrics_ShouldApplyHalfScorePenalty_WhenCompletedIsZeroAndOverdueExists()
    {
        // Arrange
        var userId = 7;
        var users = new List<User> { new() { Id = userId, Name = "Low Performer" } }.BuildMockDbSet();
        var tasks = new List<ExternalTask>
        {
            new() { Id = 1, AssigneeId = userId, Status = "InProgress", CreatedAt = DateTime.UtcNow.AddDays(-10), Deadline = DateTime.UtcNow.AddDays(-7) }
        }.BuildMockDbSet();

        var metricsMock = _metricsDb.BuildMockDbSet();
        metricsMock.Setup(m => m.Add(It.IsAny<Metric>())).Callback<Metric>(_metricsDb.Add);

        _contextMock.Setup(c => c.Users).Returns(users.Object);
        _contextMock.Setup(c => c.Tasks).Returns(tasks.Object);
        _contextMock.Setup(c => c.Metrics).Returns(metricsMock.Object);

        // Act
        await _service.CalculateMetrics();

        // Assert
        _metricsDb.Should().ContainSingle();
        _metricsDb.First().ProductivityScore.Should().BeInRange(24.0, 26.0);
        _metricsDb.First().OverdueTasks.Should().Be(1);
        _metricsDb.First().CompletedTasks.Should().Be(0);
    }

    [Fact]
    public async Task CalculateMetrics_ShouldSetScoreToZero_WhenOverdueDaysExceedMaxThreshold()
    {
        // Arrange
        var userId = 9;
        var users = new List<User> { new() { Id = userId, Name = "Overdue User" } }.BuildMockDbSet();

        var tasks = new List<ExternalTask>
        {
            new() { Id = 2, AssigneeId = userId, Status = "InProgress", CreatedAt = DateTime.UtcNow.AddDays(-30), Deadline = DateTime.UtcNow.AddDays(-20) }
        }.BuildMockDbSet();

        var metricsMock = _metricsDb.BuildMockDbSet();
        metricsMock.Setup(m => m.Add(It.IsAny<Metric>())).Callback<Metric>(_metricsDb.Add);

        _contextMock.Setup(c => c.Users).Returns(users.Object);
        _contextMock.Setup(c => c.Tasks).Returns(tasks.Object);
        _contextMock.Setup(c => c.Metrics).Returns(metricsMock.Object);

        // Act
        await _service.CalculateMetrics();

        // Assert
        _metricsDb.First().ProductivityScore.Should().Be(0.0);
    }
}
