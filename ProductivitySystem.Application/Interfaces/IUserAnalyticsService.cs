using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IUserAnalyticsService
{
    Task<EmployeeDetailsDto> GetEmployeeDetails(int userId);
}
