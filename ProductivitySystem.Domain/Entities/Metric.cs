namespace ProductivitySystem.Domain.Entities;

public class Metric
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }

    public double AvgCompletionTime { get; set; }
    public double ProductivityScore { get; set; }
}
