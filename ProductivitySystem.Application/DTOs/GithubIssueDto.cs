public class GithubIssueDto
{
    public string Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; }

    public string? Status { get; set; }

    public string? AssigneeLogin { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public DateTime? Deadline { get; set; }

    public List<string> Labels { get; set; } = new();
}
