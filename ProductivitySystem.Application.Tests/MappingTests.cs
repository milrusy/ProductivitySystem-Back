using FluentAssertions;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Mappers;

namespace ProductivitySystem.Application.Tests;

public class MappingTests
{
    [Fact]
    public void TaskMapper_ShouldCorrectlyMapGitHubIssuesToTaskDtos()
    {
        // Arrange
        var gitHubIssues = new List<GithubIssueDto>
        {
            new()
            {
                Id = "issue_01",
                Title = "Fix critical memory leak",
                Status = "In Progress",
                AssigneeLogin = "dev_john",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ClosedAt = null,
                Labels = new List<string> { "bug", "critical" }
            },
            new()
            {
                Id = "issue_02",
                Title = "Update documentation",
                Status = "Done",
                AssigneeLogin = null,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ClosedAt = DateTime.UtcNow.AddDays(-1),
                Labels = new List<string> { "documentation" }
            }
        };

        // Act
        var result = TaskMapper.Map(gitHubIssues);

        // Assert
        result.Should().HaveCount(2);

        result[0].Id.Should().Be("issue_01");
        result[0].Status.Should().Be("InProgress");
        result[0].Severity.Should().Be("Critical");
        result[0].EmployeeName.Should().Be("dev_john");
        result[0].CompletedAt.Should().BeNull();

        result[1].Id.Should().Be("issue_02");
        result[1].Status.Should().Be("Completed");
        result[1].Severity.Should().Be("Info");
        result[1].EmployeeName.Should().Be("Unassigned");
        result[1].CompletedAt.Should().NotBeNull();
    }
}