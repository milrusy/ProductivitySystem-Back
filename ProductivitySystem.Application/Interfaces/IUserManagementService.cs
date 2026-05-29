using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IUserManagementService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);

    Task<string> CreateUser(CreateUserDto dto);

    Task ChangePassword(
        int currentUserId,
        ChangePasswordDto dto);

    Task ResetPassword(ResetPasswordDto dto);
}
