using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class TrelloSyncService : ITrelloSyncService
{
    private readonly AppDbContext _context;
    private readonly ITrelloService _trello;

    public TrelloSyncService(
        AppDbContext context,
        ITrelloService trello)
    {
        _context = context;
        _trello = trello;
    }

    public async Task SyncAsync()
    {
        var cards = await _trello.GetCardsAsync();

        var trelloSource = await _context.Sources
            .FirstOrDefaultAsync(x => x.Name == "trello");

        foreach (var card in cards)
        {
            var existing = await _context.Tasks
                .FirstOrDefaultAsync(x => x.ExternalId == card.Id);

            var memberId = card.MemberIds.FirstOrDefault();


            var user = await GetOrCreateUserByTrelloMember(memberId);

            var mapping = await _context.ExternalUserMappings
                .FirstOrDefaultAsync(x => x.TrelloMemberId == memberId);

            var status =
                card.ListName == "Done" ? "Completed" :
                card.ListName == "In Progress" ? "InProgress" :
                "Backlog";

            var entity = new ExternalTask
            {
                ExternalId = card.Id,
                Title = card.Name,
                Priority = "Not set",
                Status = status,
                Deadline = card.Due,
                AssigneeId = user.Id,
                SourceId = trelloSource.Id,
                SyncedAt = DateTime.UtcNow
            };

            if (existing == null)
            {
                _context.Tasks.Add(entity);
            }
            else
            {
                existing.Title = entity.Title;
                existing.Status = entity.Status;
                existing.Deadline = entity.Deadline;
                existing.AssigneeId = entity.AssigneeId;
                existing.SyncedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<User> GetOrCreateUserByTrelloMember(
        string memberId)
    {
        var mappedUser = await _context.ExternalUserMappings
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TrelloMemberId == memberId);

        if (mappedUser?.User != null)
            return mappedUser.User;

        //var externalUser = await _context.Users
        //    .FirstOrDefaultAsync(x =>
        //        x.ExternalId == memberId);

        //if (externalUser != null)
        //    return externalUser;

        var newUser = new User
        {
            Name = $"Trello User {memberId}",
            Email = $"{memberId}@trello.local",

            ExternalId = memberId,
            IsExternal = true,

            Role = "Employee",
            PasswordHash = "external",
            DepartmentId = 1017
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return newUser;
    }
}
