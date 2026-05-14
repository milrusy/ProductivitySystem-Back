using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class UserAnalyticsService : IUserAnalyticsService
{
    private readonly AppDbContext _context;

    public UserAnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDetailsDto> GetEmployeeDetails(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var metrics = await _context.Metrics
            .FirstOrDefaultAsync(m => m.UserId == userId);

        var tasks = await _context.Tasks
            .Where(t => t.AssigneeId == userId)
            .Select(t => new EmployeeTaskDto
            {
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                Deadline = t.Deadline
            })
            .ToListAsync();

        return new EmployeeDetailsDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            Department = user.Department.Name,

            CompletedTasks = metrics?.CompletedTasks ?? 0,
            OverdueTasks = metrics?.OverdueTasks ?? 0,
            AvgCompletionTime = metrics?.AvgCompletionTime ?? 0,
            ProductivityScore = metrics?.ProductivityScore ?? 0,

            Tasks = tasks
        };
    }
}
