using ProductivitySystem.Application.DTOs;

public interface IAnalyticsService
{
    Task<List<DepartmentAnalyticsDto>> GetDepartmentAnalytics();
    Task<TaskDistributionDto> GetTaskDistribution();
}
