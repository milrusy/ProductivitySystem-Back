using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IAnalyticsService
{
    Task<List<DepartmentAnalyticsDto>> GetDepartmentAnalytics();
    Task<TaskDistributionDto> GetTaskDistribution();
}
