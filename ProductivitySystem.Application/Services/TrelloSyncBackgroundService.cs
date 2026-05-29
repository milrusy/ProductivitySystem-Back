using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductivitySystem.Application.Interfaces;

namespace ProductivitySystem.Application.Services;

public class TrelloSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrelloSyncBackgroundService> _logger;

    public TrelloSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TrelloSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            try
            {
                var sync = scope.ServiceProvider
                    .GetRequiredService<ITrelloSyncService>();

                await sync.SyncAsync();

                _logger.LogInformation("Trello sync OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trello sync failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
