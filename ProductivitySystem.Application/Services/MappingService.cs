using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Infrastructure.Data;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Services;

public class MappingService : IMappingService
{
    private readonly AppDbContext _context;

    public MappingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<object>> GetAllAsync()
    {
        return await _context.ExternalUserMappings
            .Include(x => x.User)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                UserName = x.User.Name,
                x.GitHubLogin
            })
            .ToListAsync<object>();
    }

    public async Task<object?> GetByUserIdAsync(int userId)
    {
        var mapping = await _context.ExternalUserMappings
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (mapping == null) return null;

        return new
        {
            mapping.Id,
            mapping.UserId,
            mapping.GitHubLogin
        };
    }

    public async Task UpsertAsync(ExternalUserMappingDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);

        if (user == null)
            throw new Exception("User not found");

        var existing = await _context.ExternalUserMappings
            .FirstOrDefaultAsync(x => x.UserId == dto.UserId);

        if (existing == null)
        {
            _context.ExternalUserMappings.Add(new ExternalUserMapping
            {
                UserId = dto.UserId,
                GitHubLogin = dto.GitHubLogin
            });
        }
        else
        {
            existing.GitHubLogin = dto.GitHubLogin;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var mapping = await _context.ExternalUserMappings
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mapping == null)
            throw new Exception("Mapping not found");

        _context.ExternalUserMappings.Remove(mapping);

        await _context.SaveChangesAsync();
    }
}
