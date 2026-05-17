using ProductivitySystem.Application.DTOs;
using System.Linq;

public class TaskMapper
{
    public static List<TaskDto> Map(List<GithubIssueDto> issues)
    {
        return issues.Select(i => new TaskDto
        {
            Id = i.Id,
            Title = i.Title,

            Status = MapStatus(i.Status),

            Severity =
                i.Labels.Contains("critical") ? "Critical" :
                i.Labels.Contains("warning") ? "Warning" :
                "Info",

            EmployeeName = i.AssigneeLogin ?? "Unassigned",
            CreatedAt = i.CreatedAt,
            CompletedAt = i.ClosedAt,
            Source = "github"
        }).ToList();
    }

    private static string MapStatus(string? status)
    {
        return status switch
        {
            "Backlog" => "Backlog",
            "In Progress" => "InProgress",
            "Done" => "Completed",
            _ => "Backlog"
        };
    }
}
