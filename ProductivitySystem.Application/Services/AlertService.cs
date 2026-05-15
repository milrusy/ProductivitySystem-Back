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
            // HIGH
            if (metric.OverdueTasks >= 5)
            {
                var exists = await _context.Alerts.AnyAsync(a =>
                    a.UserId == metric.UserId &&
                    a.Message.Contains("overdue"));

                if (!exists)
                {
                    _context.Alerts.Add(new Alert
                    {
                        UserId = metric.UserId,

                        Message =
                            $"Too many overdue tasks detected",

                        Severity = "Critical",

                        CreatedAt = DateTime.UtcNow,

                        IsRead = false
                    });
                }
            }

            // MEDIUM
            if (metric.ProductivityScore < 20)
            {
                var exists = await _context.Alerts.AnyAsync(a =>
                    a.UserId == metric.UserId &&
                    a.Message.Contains("Productivity dropped"));

                if (!exists)
                {
                    _context.Alerts.Add(new Alert
                    {
                        UserId = metric.UserId,

                        Message =
                            $"Productivity dropped below threshold",

                        Severity = "Warning",

                        CreatedAt = DateTime.UtcNow,

                        IsRead = false
                    });
                }
            }

            // INFO
            if (metric.ProductivityScore >= 80)
            {
                var exists = await _context.Alerts.AnyAsync(a =>
                    a.UserId == metric.UserId &&
                    a.Message.Contains("Excellent productivity"));

                if (!exists)
                {
                    _context.Alerts.Add(new Alert
                    {
                        UserId = metric.UserId,

                        Message =
                            $"Excellent productivity performance achieved",

                        Severity = "Info",

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


    public async Task<List<AlertDto>> GetUnread()
    {
        return await _context.Alerts
            .Include(a => a.User)
            .Where(a => !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AlertDto
            {
                Id = a.Id,
                Message = a.Message,
                Severity = a.Severity,
                EmployeeName = a.User!.Name,
                CreatedAt = a.CreatedAt,
                IsRead = a.IsRead
            })
            .ToListAsync();
    }

    public async Task MarkAsRead(int id)
    {
        var alert = await _context.Alerts
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alert == null)
            return;

        alert.IsRead = true;

        await _context.SaveChangesAsync();
    }
}
