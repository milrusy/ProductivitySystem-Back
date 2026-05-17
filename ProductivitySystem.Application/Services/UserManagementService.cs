using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;
using System;

namespace ProductivitySystem.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _context;

    public UserManagementService(AppDbContext context)
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


    public async Task CreateUser(CreateUserDto dto)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);

        if (exists)
        {
            throw new Exception("User already exists");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,

            PasswordHash = PasswordGenerator.Generate(),

            Role = dto.Role,
            DepartmentId = dto.DepartmentId
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();
    }

    public async Task ChangePassword(
        int currentUserId,
        ChangePasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (user.PasswordHash != dto.OldPassword)
        {
            throw new Exception("Old password incorrect");
        }

        user.PasswordHash = dto.NewPassword;

        await _context.SaveChangesAsync();
    }

    public async Task ResetPassword(ResetPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        user.PasswordHash = dto.NewPassword;

        await _context.SaveChangesAsync();
    }
}
