using Fragmenta.Dal;

namespace Fragmenta.Api.Services;

public class ArchivedBoardCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _delay = TimeSpan.FromDays(1);

    public ArchivedBoardCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var now = DateTime.UtcNow.AddDays(-30);
            var expiredItems = dbContext.Boards
                .Where(x => x.ArchivedAt <= now);

            dbContext.Boards.RemoveRange(expiredItems);
            await dbContext.SaveChangesAsync(stoppingToken);

            await Task.Delay(_delay, stoppingToken);
        }
    }
}