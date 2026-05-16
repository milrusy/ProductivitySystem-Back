namespace ProductivitySystem.Application.DTOs;

public class TaskDto
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Status { get; set; }

    public string Severity { get; set; }

    public string EmployeeName { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? Deadline { get; set; }

    public string Source { get; set; }
}
