using ProductivitySystem.Domain.Entities;
namespace ProductivitySystem.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
