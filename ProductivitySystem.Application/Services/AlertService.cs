using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _context;

    public AlertService(AppDbContext context)
    {
        _context = context;
    }

    public async Task GenerateAlerts()
    {
        var metrics = await _context.Metrics
            .Include(m => m.User)
            .ToListAsync();

        foreach (var metric in metrics)
        {
            if (metric.OverdueTasks >= 5)
            {
                var exists = await _context.Alerts.AnyAsync(a =>
                    a.UserId == metric.UserId &&
                    a.Message.Contains("overdue") &&
                    !a.IsRead);

                if (!exists)
                {
                    _context.Alerts.Add(new Alert
                    {
                        UserId = metric.UserId,

                        Message =
                            $"{metric.User.Name} has too many overdue tasks",

                        Severity = "High",

                        CreatedAt = DateTime.UtcNow,

                        IsRead = false
                    });
                }
            }

            if (metric.ProductivityScore < 20)
            {
                var exists = await _context.Alerts.AnyAsync(a =>
                    a.UserId == metric.UserId &&
                    a.Message.Contains("Productivity") &&
                    !a.IsRead);

                if (!exists)
                {
                    _context.Alerts.Add(new Alert
                    {
                        UserId = metric.UserId,

                        Message =
                            $"Productivity dropped below threshold",

                        Severity = "Medium",

                        CreatedAt = DateTime.UtcNow,

                        IsRead = false
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<AlertDto>> GetAlerts()
    {
        return await _context.Alerts
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AlertDto
            {
                Id = a.Id,

                EmployeeName = a.User.Name,

                Message = a.Message,

                Severity = a.Severity,

                IsRead = a.IsRead,

                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }
}
