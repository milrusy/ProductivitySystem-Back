using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces; 
public interface IGitHubService
{
    Task<List<GitHubTeamDto>> GetTeamsAsync();
    Task<List<GitHubUserDto>> GetTeamMembersAsync(string teamSlug);
    Task<List<GithubIssueDto>> GetIssuesAsync();
}
