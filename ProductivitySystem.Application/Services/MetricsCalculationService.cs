using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class MetricsCalculationService
    : IMetricsCalculationService
{
    private readonly AppDbContext _context;

    public MetricsCalculationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CalculateMetrics()
    {
        var users = await _context.Users.ToListAsync();

        foreach (var user in users)
        {
            var tasks = await _context.Tasks
                .Where(t => t.AssigneeId == user.Id)
                .ToListAsync();

            var completedTasks = tasks
                .Count(t => t.Status == "Done");

            var overdueTasks = tasks.Count(t =>
                t.Deadline < DateTime.UtcNow &&
                t.Status != "Done");

            var completed = tasks
                .Where(t =>
                    t.CompletedAt != null)
                .ToList();

            double avgCompletionTime = 0;

            if (completed.Any())
            {
                avgCompletionTime =
                    completed.Average(t =>
                        (t.CompletedAt!.Value - t.CreatedAt)
                        .TotalHours);
            }

            var productivityScore =
                completedTasks * 2 -
                overdueTasks * 3;

            var existingMetric =
                await _context.Metrics
                .FirstOrDefaultAsync(m =>
                    m.UserId == user.Id);

            if (existingMetric == null)
            {
                existingMetric = new Metric
                {
                    UserId = user.Id
                };

                _context.Metrics.Add(existingMetric);
            }

            existingMetric.PeriodStart =
                DateTime.UtcNow.AddDays(-30);

            existingMetric.PeriodEnd =
                DateTime.UtcNow;

            existingMetric.CompletedTasks =
                completedTasks;

            existingMetric.OverdueTasks =
                overdueTasks;

            existingMetric.AvgCompletionTime =
                avgCompletionTime;

            existingMetric.ProductivityScore =
                productivityScore;
        }

        await _context.SaveChangesAsync();
    }
}
