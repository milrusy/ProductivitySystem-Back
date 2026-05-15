using ProductivitySystem.Application.DTOs;

namespace ProductivitySystem.Application.Interfaces;

public interface IAlertService
{
    Task GenerateAlerts();

    Task<List<AlertDto>> GetAlerts();

    Task<List<AlertDto>> GetUnread();

    Task MarkAsRead(int id);
}
