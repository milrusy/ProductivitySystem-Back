using GraphQL.Common.Response;
using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Mappers;

public class GitHubMapper
{
    public static List<GithubIssueDto> Map(GraphQLResponse response)
    {
        var items = response
            .Data
            .Organization
            .ProjectV2
            .Items
            .Nodes;

        var result = new List<GithubIssueDto>();

        foreach (var item in items)
        {
            var issue = item.Content;

            if (issue == null) continue;

            var status =
                item.FieldValues?
                    .Nodes?
                    .FirstOrDefault()?
                    .Name;

            result.Add(new GithubIssueDto
            {
                Id = issue.Id,
                Number = issue.Number,
                Title = issue.Title,
                CreatedAt = issue.CreatedAt,
                ClosedAt = issue.ClosedAt,

                AssigneeLogin =
                    issue.Assignees?.Nodes?.FirstOrDefault()?.Login,

                Labels = issue.Labels?.Nodes?.ToList(),

                Status = status
            });
        }

        return result;
    }
}
