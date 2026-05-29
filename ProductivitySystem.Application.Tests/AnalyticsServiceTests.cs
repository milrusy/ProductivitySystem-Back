using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;
using ProductivitySystem.Application.Services;

namespace ProductivitySystem.Application.Tests;

public class AnalyticsServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);
        _service = new AnalyticsService(_contextMock.Object);
    }

    [Fact]
    public async Task GetDepartmentAnalytics_ShouldCorrectlyAggregateData()
    {
        // Arrange
        var departments = new List<Department>
        {
            new()
            {
                Id = 1,
                Name = "DotNet Team",
                Users = new List<User>
                {
                    new()
                    {
                        Id = 101,
                        DepartmentId = 1,
                        Tasks = new List<ExternalTask>
                        {
                            new() { Id = 1, CompletedAt = DateTime.UtcNow }, // Completed
                            new() { Id = 2, CompletedAt = null, Deadline = DateTime.UtcNow.AddDays(-2) } // Overdue
                        },
                        Metrics = new List<Metric>
                        {
                            new() { Id = 1, ProductivityScore = 80.0 },
                            new() { Id = 2, ProductivityScore = 60.0 }
                        }
                    }
                }
            }
        };

        var depMock = departments.BuildMockDbSet();
        _contextMock.Setup(c => c.Departments).Returns(depMock.Object);

        // Act
        var result = await _service.GetDepartmentAnalytics();

        // Assert
        result.Should().ContainSingle();
        var analytics = result.First();
        analytics.DepartmentName.Should().Be("DotNet Team");
        analytics.CompletedTasks.Should().Be(1);
        analytics.OverdueTasks.Should().Be(1);
        analytics.AverageProductivity.Should().Be(70.0);
    }
}
