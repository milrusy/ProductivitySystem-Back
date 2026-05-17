using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public GitHubService(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;

        _client.DefaultRequestHeaders.UserAgent.ParseAdd("ProductivityApp");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config["GitHub:Token"]);
    }

    // =========================
    // GRAPHQL ISSUES (PROJECT)
    // =========================
    public async Task<List<GithubIssueDto>> GetIssuesAsync()
    {
        var query = """
        query {
          organization(login: "Productivity-System") {
            projectV2(number: 1) {
              items(first: 50) {
                nodes {
                  content {
                    ... on Issue {
                      id
                      number
                      title
                      createdAt
                      closedAt

                      assignees(first: 1) {
                        nodes { login }
                      }

                      labels(first: 10) {
                        nodes { name }
                      }
                    }
                  }

                  fieldValues(first: 20) {
                        nodes {
                        ... on ProjectV2ItemFieldDateValue {
                            date
                            field {
                            ... on ProjectV2FieldCommon {
                                name
                            }
                            }
                        }

                        ... on ProjectV2ItemFieldSingleSelectValue {
                            name
                        }
                    }
                  }
                }
              }
            }
          }
        }
        """;

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.github.com/graphql"
        );

        request.Content = new StringContent(
            JsonSerializer.Serialize(new { query }),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var nodes = doc.RootElement
            .GetProperty("data")
            .GetProperty("organization")
            .GetProperty("projectV2")
            .GetProperty("items")
            .GetProperty("nodes");

        var result = new List<GithubIssueDto>();

        foreach (var node in nodes.EnumerateArray())
        {
            var content = node.GetProperty("content");

            if (content.ValueKind == JsonValueKind.Null)
                continue;

            // -------------------------
            // LABELS SAFE
            // -------------------------
            var labels = new List<string>();

            if (content.TryGetProperty("labels", out var labelsObj))
            {
                foreach (var l in labelsObj.GetProperty("nodes").EnumerateArray())
                {
                    var name = l.GetProperty("name").GetString();
                    if (!string.IsNullOrEmpty(name))
                        labels.Add(name);
                }
            }

            // -------------------------
            // ASSIGNEE SAFE
            // -------------------------
            string? assignee = null;

            if (content.TryGetProperty("assignees", out var assigneesObj))
            {
                assignee = assigneesObj
                    .GetProperty("nodes")
                    .EnumerateArray()
                    .FirstOrDefault()
                    .TryGetProperty("login", out var login)
                        ? login.GetString()
                        : null;
            }

            // -------------------------
            // STATUS SAFE (Project field)
            // -------------------------
            string? status = null;

            if (node.TryGetProperty("fieldValues", out var fv))
            {
                foreach (var n in fv.GetProperty("nodes").EnumerateArray())
                {
                    if (n.ValueKind == JsonValueKind.Object &&
                        n.TryGetProperty("name", out var nameProp))
                    {
                        status = nameProp.GetString();
                        if (!string.IsNullOrEmpty(status))
                            break;
                    }
                }
            }

            DateTime? deadline = null;

            if (node.TryGetProperty("fieldValues", out var fv2))
            {
                foreach (var field in fv2.GetProperty("nodes").EnumerateArray())
                {
                    if (field.ValueKind != JsonValueKind.Object)
                        continue;

                    if (!field.TryGetProperty("date", out var dateProp))
                        continue;

                    if (dateProp.ValueKind != JsonValueKind.String)
                        continue;

                    var fieldName =
                        field.GetProperty("field")
                              .GetProperty("name")
                              .GetString();

                    if (fieldName == "Target date")
                    {
                        if (DateTime.TryParse(dateProp.GetString(), out var dt))
                            deadline = dt;
                    }
                }
            }

            // -------------------------
            // DATES SAFE
            // -------------------------
            DateTime? createdAt = null;
            DateTime? closedAt = null;

            if (content.TryGetProperty("createdAt", out var createdProp) &&
                createdProp.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(createdProp.GetString(), out var created))
            {
                createdAt = created;
            }

            if (content.TryGetProperty("closedAt", out var closedProp) &&
                closedProp.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(closedProp.GetString(), out var closed))
            {
                closedAt = closed;
            }

            // -------------------------
            // BUILD DTO
            // -------------------------
            result.Add(new GithubIssueDto
            {
                Id = content.GetProperty("id").GetString(),
                Number = content.GetProperty("number").GetInt32(),
                Title = content.GetProperty("title").GetString(),

                CreatedAt = createdAt ?? DateTime.UtcNow,
                ClosedAt = closedAt,
                Deadline = deadline,

                AssigneeLogin = assignee,
                Status = status,
                Labels = labels
            });
        }

        return result;
    }

    // =========================
    // REST TEAMS
    // =========================
    public async Task<List<GitHubTeamDto>> GetTeamsAsync()
    {
        var org = "Productivity-System";

        var json = await _client.GetStringAsync(
            $"https://api.github.com/orgs/{org}/teams"
        );

        var doc = JsonDocument.Parse(json);

        var result = new List<GitHubTeamDto>();

        foreach (var t in doc.RootElement.EnumerateArray())
        {
            result.Add(new GitHubTeamDto
            {
                Slug = t.GetProperty("slug").GetString(),
                Name = t.GetProperty("name").GetString()
            });
        }

        return result;
    }

    // =========================
    // REST TEAM MEMBERS
    // =========================
    public async Task<List<GitHubUserDto>> GetTeamMembersAsync(string teamSlug)
    {
        var org = "Productivity-System";

        var json = await _client.GetStringAsync(
            $"https://api.github.com/orgs/{org}/teams/{teamSlug}/members"
        );

        var doc = JsonDocument.Parse(json);

        var result = new List<GitHubUserDto>();

        foreach (var u in doc.RootElement.EnumerateArray())
        {
            result.Add(new GitHubUserDto
            {
                Login = u.GetProperty("login").GetString()
            });
        }

        return result;
    }
}
