using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using ProductivitySystem.Application.Services;
using System.Net;
namespace ProductivitySystem.Application.Tests;

public class GitHubServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IConfiguration> _configMock;

    public GitHubServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["GitHub:Token"]).Returns("mock_github_token_xyz");
    }

    [Fact]
    public async Task GetIssuesAsync_WithValidGraphQLJson_ShouldCorrectlyParseAndReturnDtos()
    {
        // Arrange
        var fakeGraphQLResponse = """
        {
          "data": {
            "organization": {
              "projectV2": {
                "items": {
                  "nodes": [
                    {
                      "content": {
                        "id": "I_111",
                        "number": 42,
                        "title": "Implement feature X",
                        "createdAt": "2026-05-20T10:00:00Z",
                        "closedAt": null,
                        "assignees": { "nodes": [{ "login": "octocat" }] },
                        "labels": { "nodes": [{ "name": "critical" }] }
                      },
                      "fieldValues": {
                        "nodes": [
                          { "name": "In Progress" },
                          { "date": "2026-06-01", "field": { "name": "Target date" } }
                        ]
                      }
                    }
                  ]
                }
              }
            }
          }
        }
        """;

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString().Contains("graphql")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeGraphQLResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(_handlerMock.Object);
        var service = new GitHubService(httpClient, _configMock.Object);

        // Act
        var result = await service.GetIssuesAsync();

        // Assert
        result.Should().ContainSingle();
        var issue = result.First();
        issue.Id.Should().Be("I_111");
        issue.Number.Should().Be(42);
        issue.Title.Should().Be("Implement feature X");
        issue.AssigneeLogin.Should().Be("octocat");
        issue.Status.Should().Be("In Progress");
        issue.Labels.Should().Contain("critical");
        issue.Deadline.Should().Be(new DateTime(2026, 06, 01));
    }

    [Fact]
    public async Task GetTeamsAsync_ShouldParseRestApiArrayCorrectly()
    {
        // Arrange
        var fakeRestResponse = """
        [
          { "slug": "dev-team", "name": "Development Team" },
          { "slug": "qa-team", "name": "Quality Assurance Team" }
        ]
        """;

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("teams")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeRestResponse, System.Text.Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(_handlerMock.Object);
        var service = new GitHubService(httpClient, _configMock.Object);

        // Act
        var result = await service.GetTeamsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Slug.Should().Be("dev-team");
        result[0].Name.Should().Be("Development Team");
    }
}
