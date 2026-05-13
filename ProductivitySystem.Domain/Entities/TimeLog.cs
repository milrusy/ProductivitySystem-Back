namespace ProductivitySystem.Domain.Entities;

public class TimeLog
{
    public int Id { get; set; }

    public int TaskId { get; set; }
    public ExternalTask Task { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public double TimeSpent { get; set; }
    public DateTime LogDate { get; set; }
}
