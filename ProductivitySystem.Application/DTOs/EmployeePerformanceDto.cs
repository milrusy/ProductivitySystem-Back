namespace ProductivitySystem.Application.DTOs; 

public class EmployeePerformanceDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }

    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }

    public double ProductivityScore { get; set; }
}
