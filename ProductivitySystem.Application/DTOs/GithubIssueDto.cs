public class GithubIssueDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string State { get; set; }
    public List<GithubLabel> Labels { get; set; }
    public GithubUser Assignee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class GithubLabel
{
    public string Name { get; set; }
}

public class GithubUser
{
    public string Login { get; set; }
}
