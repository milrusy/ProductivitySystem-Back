using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
}
