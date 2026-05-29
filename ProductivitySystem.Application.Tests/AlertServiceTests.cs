using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Tests;

public class AlertServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly AlertService _service;
    private readonly List<Alert> _alertsDb;

    public AlertServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);

        _alertsDb = new List<Alert>();
        _service = new AlertService(_contextMock.Object);
    }

    [Fact]
    public async Task GenerateAlerts_ShouldCreateCriticalAlert_WhenOverdueTasksAreFiveOrMore()
    {
        // Arrange
        var metrics = new List<Metric>
        {
            new() { Id = 1, UserId = 10, OverdueTasks = 5, ProductivityScore = 50, User = new User { Id = 10, Name = "John Doe" } }
        };

        var metricsMock = metrics.BuildMockDbSet();
        var alertsMock = _alertsDb.BuildMockDbSet();

        alertsMock.Setup(m => m.Add(It.IsAny<Alert>())).Callback<Alert>(_alertsDb.Add);

        _contextMock.Setup(c => c.Metrics).Returns(metricsMock.Object);
        _contextMock.Setup(c => c.Alerts).Returns(alertsMock.Object);

        // Act
        await _service.GenerateAlerts();

        // Assert
        _alertsDb.Should().ContainSingle();
        _alertsDb.First().UserId.Should().Be(10);
        _alertsDb.First().Severity.Should().Be("Critical");
        _alertsDb.First().Message.Should().Contain("Too many overdue tasks");
    }

    [Fact]
    public async Task GenerateAlerts_ShouldNotCreateDuplicateAlert_IfAlertAlreadyExists()
    {
        // Arrange
        var metrics = new List<Metric>
        {
            new() { Id = 1, UserId = 10, OverdueTasks = 6, ProductivityScore = 50, User = new User { Id = 10, Name = "John Doe" } }
        };

        _alertsDb.Add(new Alert { Id = 1, UserId = 10, Message = "Too many overdue tasks detected", Severity = "Critical" });

        var metricsMock = metrics.BuildMockDbSet();
        var alertsMock = _alertsDb.BuildMockDbSet();

        _contextMock.Setup(c => c.Metrics).Returns(metricsMock.Object);
        _contextMock.Setup(c => c.Alerts).Returns(alertsMock.Object);

        // Act
        await _service.GenerateAlerts();

        // Assert
        _alertsDb.Should().HaveCount(1);
    }
}