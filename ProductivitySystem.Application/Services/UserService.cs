using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Infrastructure.Data;
using System;

namespace ProductivitySystem.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            })
            .FirstOrDefaultAsync();
    }
}
