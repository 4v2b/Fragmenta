using Fragmenta.Api.Contracts;
using Fragmenta.Dal;

namespace Fragmenta.Api.Services;

public class BoardCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _delay = TimeSpan.FromDays(1);

    public BoardCleanupBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var cleanupService = scope.ServiceProvider.GetRequiredService<IBoardService>();

            await cleanupService.CleanupArchivedBoardsAsync(stoppingToken);

            await Task.Delay(_delay, stoppingToken);
        }
    }
}