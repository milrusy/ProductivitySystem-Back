public interface IGitHubService
{
    Task<List<GitHubTeamDto>> GetTeamsAsync();
    Task<List<GitHubUserDto>> GetTeamMembersAsync(string teamSlug);
    Task<List<GithubIssueDto>> GetIssuesAsync();
}
