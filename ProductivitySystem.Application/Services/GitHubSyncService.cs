using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class GitHubSyncService
{
    private readonly AppDbContext _context;
    private readonly IGitHubService _gitHubService;

    public GitHubSyncService(
        AppDbContext context,
        IGitHubService gitHubService)
    {
        _context = context;
        _gitHubService = gitHubService;
    }

    public async Task SyncDepartments()
    {
        var teams = await _gitHubService.GetTeamsAsync();

        foreach (var team in teams)
        {
            var existing = await _context.Departments
                .FirstOrDefaultAsync(d => d.ExternalId == team.Slug);

            if (existing == null)
            {
                _context.Departments.Add(new Department
                {
                    Name = team.Name,
                    ExternalId = team.Slug
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task SyncUsers()
    {
        //var teams = await _gitHubService.GetTeamsAsync();

        //foreach (var team in teams)
        //{
        //    var department = await _context.Departments
        //        .FirstOrDefaultAsync(d => d.ExternalId == team.Slug);

        //    var members = await _gitHubService.GetTeamMembersAsync(team.Slug);

        //    foreach (var member in members)
        //    {
        //        var user = await _context.Users
        //            .FirstOrDefaultAsync(u => u.Email == member.Login);

        //        if (user == null)
        //        {
        //            user = new User
        //            {
        //                Name = member.Login,
        //                Email = member.Login,
        //                DepartmentId = department.Id,
        //                Role = "Employee"
        //            };

        //            _context.Users.Add(user);
        //        }
        //        else
        //        {
        //            user.DepartmentId = department.Id;
        //        }
        //    }
        //}

        //await _context.SaveChangesAsync();
    }
}