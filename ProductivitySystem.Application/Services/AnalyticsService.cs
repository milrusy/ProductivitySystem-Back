using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentAnalyticsDto>> GetDepartmentAnalytics()
    {
        var data = await _context.Departments
            .Include(d => d.Users)
                .ThenInclude(u => u.Tasks)
            .Include(d => d.Users)
                .ThenInclude(u => u.Metrics)
            .ToListAsync();

        return data.Select(d => new DepartmentAnalyticsDto
        {
            DepartmentName = d.Name,

            CompletedTasks = d.Users?
                .SelectMany(u => u.Tasks ?? new List<ExternalTask>())
                .Count(t => t.CompletedAt != null) ?? 0,

            OverdueTasks = d.Users?
                .SelectMany(u => u.Tasks ?? new List<ExternalTask>())
                .Count(t => t.CompletedAt == null && t.Deadline < DateTime.UtcNow) ?? 0,

            AverageProductivity = d.Users != null && d.Users.Any()
                ? d.Users
                    .SelectMany(u => u.Metrics ?? new List<Metric>())
                    .Any()
                    ? d.Users
                        .SelectMany(u => u.Metrics ?? new List<Metric>())
                        .Average(m => m.ProductivityScore)
                    : 0
                : 0
        }).ToList();
    }

    public async Task<TaskDistributionDto> GetTaskDistribution()
    {
        var tasks = await _context.Tasks.ToListAsync();

        return new TaskDistributionDto
        {
            Completed = tasks.Count(t => t.CompletedAt != null),
            Overdue = tasks.Count(t => t.CompletedAt == null && t.Deadline < DateTime.UtcNow),
            InProgress = tasks.Count(t => t.CompletedAt == null && (t.Deadline >= DateTime.UtcNow || t.Deadline == null))
        };
    }
}
