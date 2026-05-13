using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly AppDbContext _context;

    public MetricsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MetricsDto> GetUserMetrics(int userId, DateTime? from, DateTime? to)
    {
        var tasks = _context.Tasks.Where(t => t.AssigneeId == userId);

        if (from.HasValue)
            tasks = tasks.Where(t => t.CreatedAt >= from.Value);

        if (to.HasValue)
            tasks = tasks.Where(t => t.CreatedAt <= to.Value);

        var taskList = await tasks.ToListAsync();

        var completed = taskList.Count(t => t.Status == "Done");

        var overdue = taskList.Count(t =>
            t.Deadline != null &&
            t.CompletedAt != null &&
            t.CompletedAt > t.Deadline);

        var completedTasks = taskList
            .Where(t => t.CompletedAt != null)
            .ToList();

        double avgTime = 0;

        if (completedTasks.Any())
        {
            avgTime = completedTasks
                .Average(t => (t.CompletedAt.Value - t.CreatedAt).TotalHours);
        }

        double score = 0;

        if (taskList.Any())
        {
            var completionRate = (double)completed / taskList.Count;
            var overdueRate = (double)overdue / taskList.Count;

            score = (completionRate * 0.6) + ((1 - overdueRate) * 0.4);
        }

        return new MetricsDto
        {
            CompletedTasks = completed,
            OverdueTasks = overdue,
            AvgCompletionTime = Math.Round(avgTime, 2),
            ProductivityScore = Math.Round(score, 2)
        };
    }
    public async Task<List<TrendDto>> GetTrends(int? userId, int? departmentId)
    {
        var query = _context.Tasks
            .Include(t => t.Assignee)
            .Where(t => t.CompletedAt != null)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == userId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(t =>
                t.Assignee.DepartmentId == departmentId.Value);
        }
        var tasks = await query.ToListAsync();

        var grouped = tasks
            .GroupBy(t => t.CompletedAt!.Value.Date)
            .Select(g => new TrendDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Score = Math.Round(
                    g.Average(t =>
                        t.CompletedAt <= t.Deadline ? 1.0 : 0.5
                    ),
                    2
                )
            })
            .OrderBy(x => x.Date)
            .ToList();

        return grouped;
    }
}
