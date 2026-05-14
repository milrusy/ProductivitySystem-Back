using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IAlertService
{
    Task GenerateAlerts();

    Task<List<AlertDto>> GetAlerts();
}
