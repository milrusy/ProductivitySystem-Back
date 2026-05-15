namespace ProductivitySystem.Domain.Entities;

public class Alert
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; }

    public string Message { get; set; }

    public string Severity { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; }
}
