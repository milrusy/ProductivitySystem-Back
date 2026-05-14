namespace ProductivitySystem.Application.DTOs;

public class EmployeeTaskDto
{
    public string Title { get; set; }

    public string Status { get; set; }

    public string Priority { get; set; }

    public DateTime? Deadline { get; set; }
}
