namespace ProductivitySystem.Domain.Entities;

public class ExternalTask
{
    public int Id { get; set; }
    public string ExternalId { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }

    public int AssigneeId { get; set; }
    public User Assignee { get; set; }

    public int SourceId { get; set; }
    public ExternalSource Source { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? CompletedAt { get; set; }

    public double? EstimatedTime { get; set; }

    public ICollection<TimeLog> TimeLogs { get; set; }
}
