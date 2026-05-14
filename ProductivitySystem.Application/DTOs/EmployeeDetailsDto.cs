namespace ProductivitySystem.Application.DTOs;

public class EmployeeDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Role { get; set; }

    public string Department { get; set; }

    public int CompletedTasks { get; set; }

    public int OverdueTasks { get; set; }

    public double AvgCompletionTime { get; set; }

    public double ProductivityScore { get; set; }

    public List<EmployeeTaskDto> Tasks { get; set; }
}
