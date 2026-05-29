using ProductivitySystem.Domain.Entities;

public class ExternalUserMapping
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string? GitHubLogin { get; set; }
    public string? TrelloMemberId { get; set; }
}
