using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Application.Services;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.UnitTests.Application;

public class GitHubSyncBackgroundServiceTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    private readonly GitHubSyncService _realSyncService;
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IGitHubService> _githubServiceMock;
    private readonly Mock<ILogger<GitHubSyncBackgroundService>> _loggerMock;

    public GitHubSyncBackgroundServiceTests()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>().Options;
        _contextMock = new Mock<AppDbContext>(options);

        _githubServiceMock = new Mock<IGitHubService>();
        _loggerMock = new Mock<ILogger<GitHubSyncBackgroundService>>();

        _realSyncService = new GitHubSyncService(_contextMock.Object, _githubServiceMock.Object);

        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);

        _serviceProviderMock.Setup(p => p.GetService(typeof(GitHubSyncService))).Returns(_realSyncService);
        _serviceProviderMock.Setup(p => p.GetService(typeof(AppDbContext))).Returns(_contextMock.Object);
        _serviceProviderMock.Setup(p => p.GetService(typeof(IGitHubService))).Returns(_githubServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFetchIssuesAndAddOrUpdateTasksInDatabase()
    {
        // Arrange
        var sources = new List<ExternalSource> { new() { Id = 1, Name = "github" } }.BuildMockDbSet();
        var tasks = new List<ExternalTask>().BuildMockDbSet();
        var users = new List<User> { new() { Id = 5, ExternalId = "dev_octo", Role = "Employee" } }.BuildMockDbSet();
        var departments = new List<Department>().BuildMockDbSet();

        _contextMock.Setup(c => c.Sources).Returns(sources.Object);
        _contextMock.Setup(c => c.Tasks).Returns(tasks.Object);
        _contextMock.Setup(c => c.Users).Returns(users.Object);
        _contextMock.Setup(c => c.Departments).Returns(departments.Object);

        var fakeTeams = new List<GitHubTeamDto> { new() { Slug = "devs", Name = "Developers" } };
        var fakeIssues = new List<GithubIssueDto>
        {
            new() { Id = "gh_task_99", Title = "Critical bug fix", Status = "Done", AssigneeLogin = "dev_octo", CreatedAt = DateTime.UtcNow }
        };

        _githubServiceMock.Setup(g => g.GetTeamsAsync()).ReturnsAsync(fakeTeams);
        _githubServiceMock.Setup(g => g.GetIssuesAsync()).ReturnsAsync(fakeIssues);

        var backgroundService = new GitHubSyncBackgroundService(_scopeFactoryMock.Object, _loggerMock.Object);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        // Act
        await backgroundService.StartAsync(cts.Token);
        await Task.Delay(300);
        await backgroundService.StopAsync(CancellationToken.None);

        // Assert
        _contextMock.Verify(c => c.Tasks.Add(It.Is<ExternalTask>(t => t.ExternalId == "gh_task_99")), Times.Once);
        _contextMock.Verify(c => c.Departments.Add(It.Is<Department>(d => d.ExternalId == "devs")), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }
}
