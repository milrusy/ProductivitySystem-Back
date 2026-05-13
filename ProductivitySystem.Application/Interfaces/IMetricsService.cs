using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IMetricsService
{
    Task<MetricsDto> GetUserMetrics(int userId, DateTime? from, DateTime? to);
    Task<List<TrendDto>> GetTrends(int? userId, int? departmentId);
}
