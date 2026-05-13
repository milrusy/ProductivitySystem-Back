namespace ProductivitySystem.Application.DTOs;

public class MetricsDto
{
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double AvgCompletionTime { get; set; }
    public double ProductivityScore { get; set; }
}
