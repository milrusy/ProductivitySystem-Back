using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class MetricsCalculationService : IMetricsCalculationService
{
    private readonly AppDbContext _context;

    private const double MinOverdueDays = 0.0;
    private const double MaxOverdueDays = 14.0;

    public MetricsCalculationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CalculateMetrics()
    {
        var users = await _context.Users.ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();

        var allTasks = await _context.Tasks
            .Where(t => userIds.Contains(t.AssigneeId))
            .ToListAsync();

        var existingMetrics = await _context.Metrics.ToListAsync();

        foreach (var user in users)
        {
            var userTasks = allTasks.Where(t => t.AssigneeId == user.Id).ToList();

            var completedTasksCount = userTasks.Count(t => t.Status == "Done");

            var overdueTasksCount = userTasks.Count(t =>
                t.Deadline < DateTime.UtcNow &&
                t.Status != "Done");

            var completedTasksList = userTasks.Where(t => t.CompletedAt != null).ToList();
            double avgCompletionTime = 0;
            if (completedTasksList.Any())
            {
                avgCompletionTime = completedTasksList.Average(t =>
                    (t.CompletedAt!.Value - t.CreatedAt).TotalHours);
            }

            double totalOverdueDays = userTasks
                .Where(t => t.Deadline < DateTime.UtcNow && t.Status != "Done" && t.Deadline.HasValue)
                .Sum(t => (DateTime.UtcNow - t.Deadline!.Value).TotalDays);

            double productivityScore = 100.0;

            if (totalOverdueDays > MinOverdueDays)
            {
                if (totalOverdueDays >= MaxOverdueDays)
                {
                    productivityScore = 0.0;
                }
                else
                {
                    double penaltyFactor = (totalOverdueDays - MinOverdueDays) / (MaxOverdueDays - MinOverdueDays);
                    productivityScore = (1.0 - penaltyFactor) * 100.0;
                }
            }

            if (completedTasksCount == 0 && overdueTasksCount > 0)
            {
                productivityScore *= 0.5;
            }

            productivityScore = Math.Round(productivityScore, 2);

            var metric = existingMetrics.FirstOrDefault(m => m.UserId == user.Id);

            if (metric == null)
            {
                metric = new Metric { UserId = user.Id };
                _context.Metrics.Add(metric);
            }

            metric.PeriodStart = DateTime.UtcNow.AddDays(-30);
            metric.PeriodEnd = DateTime.UtcNow;
            metric.CompletedTasks = completedTasksCount;
            metric.OverdueTasks = overdueTasksCount;
            metric.AvgCompletionTime = Math.Round(avgCompletionTime, 2);
            metric.ProductivityScore = productivityScore;
        }

        await _context.SaveChangesAsync();
    }
}
