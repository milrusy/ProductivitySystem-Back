using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public GitHubService(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;

        _client.BaseAddress = new Uri(_config["GitHub:BaseUrl"]);
        _client.DefaultRequestHeaders.Add("User-Agent", "ProductivityApp");
        _client.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {_config["GitHub:Token"]}"
        );
    }

    public async Task<List<GithubIssueDto>> GetIssuesAsync()
    {
        var org = "Productivity-System";
        var owner = _config["GitHub:Owner"];
        var repo = _config["GitHub:Repo"];

        var url = $"/repos/{org}/{repo}/issues";

        var response = await _client.GetStringAsync(url);

        return JsonSerializer.Deserialize<List<GithubIssueDto>>(response,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();
    }
    public async Task<List<GitHubTeamDto>> GetTeamsAsync()
    {
        var org = "Productivity-System";

        var response =
            await _client.GetFromJsonAsync<List<GitHubTeamResponse>>(
                $"orgs/{org}/teams"
            );

        return response.Select(t => new GitHubTeamDto
        {
            Slug = t.slug,
            Name = t.name
        }).ToList();
    }

    public async Task<List<GitHubUserDto>> GetTeamMembersAsync(string teamSlug)
    {
        var org = "Productivity-System";

        var response =
            await _client.GetFromJsonAsync<List<GitHubUserResponse>>(
                $"orgs/{org}/teams/{teamSlug}/members"
            );

        return response.Select(u => new GitHubUserDto
        {
            Login = u.login
        }).ToList();
    }

    private class GitHubTeamResponse
    {
        public string slug { get; set; }
        public string name { get; set; }
    }

    private class GitHubUserResponse
    {
        public string login { get; set; }
    }
}
