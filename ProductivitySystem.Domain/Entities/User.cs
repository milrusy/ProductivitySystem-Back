namespace ProductivitySystem.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string? ExternalId { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string? PasswordHash { get; set; }

    public string Role { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; }

    public bool IsExternal { get; set; }

    public ICollection<ExternalTask> Tasks { get; set; } = new List<ExternalTask>();
    public ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}
