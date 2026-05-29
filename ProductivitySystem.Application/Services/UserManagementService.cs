using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Domain.Entities;
using ProductivitySystem.Infrastructure.Data;

namespace ProductivitySystem.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

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

    public async Task<string> CreateUser(CreateUserDto dto)
    {
        var exists = await _context.Users
            .AnyAsync(x => x.Email == dto.Email);

        if (exists)
            throw new Exception("User with this email already exists");

        var tempPassword = GeneratePassword();

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            DepartmentId = dto.DepartmentId,
            Role = dto.Role
        };

        user.PasswordHash = _hasher.HashPassword(user, tempPassword);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return tempPassword;
    }

    public async Task ChangePassword(int currentUserId, ChangePasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user == null)
            throw new Exception("User not found");

        var verify = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.OldPassword
        );

        if (verify == PasswordVerificationResult.Failed)
            throw new Exception("Old password incorrect");

        user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);

        await _context.SaveChangesAsync();
    }

    public async Task ResetPassword(ResetPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (user == null)
            throw new Exception("User not found");

        user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);

        await _context.SaveChangesAsync();
    }

    private string GeneratePassword()
    {
        return Guid.NewGuid().ToString("N")[..10];
    }
}
