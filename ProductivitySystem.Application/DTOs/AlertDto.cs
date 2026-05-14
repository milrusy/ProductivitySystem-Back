namespace ProductivitySystem.Application.DTOs;

public class AlertDto
{
    public int Id { get; set; }

    public string EmployeeName { get; set; }

    public string Message { get; set; }

    public string Severity { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
