using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

public class GitHubSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GitHubSyncBackgroundService> _logger;

    public GitHubSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<GitHubSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            try
            {
                var syncService = scope.ServiceProvider.GetRequiredService<GitHubSyncService>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var gitHub = scope.ServiceProvider.GetRequiredService<IGitHubService>();

                _logger.LogInformation("GitHub sync started");

                // 1. Sync structure
                await syncService.SyncDepartments();
                await syncService.SyncUsers();

                // 2. Get source
                var githubSource = await db.Sources
                    .FirstOrDefaultAsync(x => x.Name == "github", stoppingToken);

                if (githubSource == null)
                {
                    _logger.LogWarning("GitHub source not found");
                    continue;
                }

                // 3. Get issues
                var issues = await gitHub.GetIssuesAsync();

                _logger.LogInformation("Fetched {count} issues", issues.Count);

                foreach (var issue in issues)
                {
                    var externalId = issue.Id.ToString();

                    // IMPORTANT: correct table
                    var existing = await db.Tasks
                        .FirstOrDefaultAsync(x => x.ExternalId == externalId, stoppingToken);

                    // SAFE assignee handling
                    var login = issue.Assignee?.Login;

                    var user = await GetOrCreateUserAsync(db, login);

                    var mapped = new ExternalTask
                    {
                        ExternalId = externalId,
                        Title = issue.Title ?? "No title",

                        Status = issue.State == "closed"
                            ? "Completed"
                            : "InProgress",

                        Priority =
                            issue.Labels?.Any(l => l.Name == "critical") == true ? "Critical" :
                            issue.Labels?.Any(l => l.Name == "high") == true ? "High" :
                            issue.Labels?.Any(l => l.Name == "low") == true ? "Low" :
                            "Medium",

                        AssigneeId = user.Id,
                        SourceId = githubSource.Id,

                        CreatedAt = issue.CreatedAt,
                        CompletedAt = issue.ClosedAt,

                        SyncedAt = DateTime.UtcNow
                    };

                    if (existing == null)
                    {
                        db.Tasks.Add(mapped);
                    }
                    else
                    {
                        existing.Title = mapped.Title;
                        existing.Status = mapped.Status;
                        existing.Priority = mapped.Priority;
                        existing.AssigneeId = mapped.AssigneeId;
                        existing.SourceId = mapped.SourceId;
                        existing.CreatedAt = mapped.CreatedAt;
                        existing.CompletedAt = mapped.CompletedAt;
                        existing.SyncedAt = DateTime.UtcNow;
                    }
                }

                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("GitHub sync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GitHub sync failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }

    private async Task<User> GetOrCreateUserAsync(
        AppDbContext db,
        string externalLogin)
    {
        if (string.IsNullOrWhiteSpace(externalLogin))
        {
            return await db.Users.FirstAsync(
                u => u.Role == "Employee");
        }

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.ExternalId == externalLogin);

        if (user != null)
            return user;

        var defaultDepartment = await db.Departments
            .FirstOrDefaultAsync(d => d.Name == "GitHub");

        if (defaultDepartment == null)
        {
            defaultDepartment = new Department
            {
                Name = "GitHub",
                ExternalId = "GitHub"
            };

            db.Departments.Add(defaultDepartment);
            await db.SaveChangesAsync();
        }

        user = new User
        {
            Name = externalLogin,
            Email = $"{externalLogin}@github.local",
            ExternalId = externalLogin,
            IsExternal = true,
            Role = "Employee",
            PasswordHash = "external", // IMPORTANT FIX
            DepartmentId = defaultDepartment.Id
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }
}
