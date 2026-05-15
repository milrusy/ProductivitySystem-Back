namespace ProductivitySystem.Application.DTOs;

public class DepartmentAnalyticsDto
{
    public string DepartmentName { get; set; }

    public int EmployeesCount { get; set; }

    public int CompletedTasks { get; set; }

    public int OverdueTasks { get; set; }

    public double AverageProductivity { get; set; }
}
