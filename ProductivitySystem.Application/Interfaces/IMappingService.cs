using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IMappingService
{
    Task<List<object>> GetAllAsync();
    Task<object?> GetByUserIdAsync(int userId);
    Task UpsertAsync(ExternalUserMappingDto dto);
    Task DeleteAsync(int id);
}
