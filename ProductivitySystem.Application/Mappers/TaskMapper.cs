using ProductivitySystem.Application.DTOs;

public class TaskMapper
{
    public static List<TaskDto> Map(List<GithubIssueDto> issues)
    {
        return issues.Select(i => new TaskDto
        {
            Id = i.Id.ToString(),
            Title = i.Title,
            Status = i.State == "closed" ? "Completed" : "InProgress",

            Severity =
                i.Labels.Any(l => l.Name == "critical") ? "Critical" :
                i.Labels.Any(l => l.Name == "warning") ? "Warning" : "Info",

            EmployeeName = i.Assignee?.Login ?? "Unassigned",
            CreatedAt = i.CreatedAt,
            CompletedAt = i.ClosedAt,
            Source = "github"
        }).ToList();
    }
}
