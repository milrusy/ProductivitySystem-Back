using Moq;
using FluentAssertions;
using MockQueryable.Moq;
using System.Text;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.UnitTests.Application;

public class ReportServiceTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);
        _service = new ReportService(_contextMock.Object);
    }

    [Fact]
    public async Task GenerateMetricsCsv_ShouldReturnValidCsvStructureAndData()
    {
        // Arrange
        var metrics = new List<Metric>
        {
            new() {
                CompletedTasks = 10,
                OverdueTasks = 2,
                AvgCompletionTime = 12.5,
                ProductivityScore = 85.3,
                User = new User { Name = "John Developer" }
            }
        };

        var metricsMock = metrics.BuildMockDbSet();
        _contextMock.Setup(c => c.Metrics).Returns(metricsMock.Object);

        // Act
        var bytes = await _service.GenerateMetricsCsv();
        var csvText = Encoding.UTF8.GetString(bytes);

        // Assert
        csvText.Should().Contain("Employee,CompletedTasks,OverdueTasks,AvgCompletionTime,ProductivityScore");
        csvText.Should().Contain("John Developer,10,2,12.5,85.3");
    }

    [Fact]
    public async Task GenerateMetricsPdf_ShouldExecuteWithoutExceptions_WhenBase64IsValid()
    {
        // Arrange
        var metrics = new List<Metric>
        {
            new() { CompletedTasks = 1, OverdueTasks = 0, User = new User { Name = "Tester" } }
        };
        var metricsMock = metrics.BuildMockDbSet();
        _contextMock.Setup(c => c.Metrics).Returns(metricsMock.Object);

        var validBase64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
        var dto = new ExportPdfDto { ChartImage = validBase64Image };

        // Act
        Func<Task<byte[]>> act = async () => await _service.GenerateMetricsPdf(dto);

        // Assert
        await act.Should().NotThrowAsync();
        var pdfBytes = await act();
        pdfBytes.Should().NotBeEmpty();
    }
}
