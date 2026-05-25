using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Risen.DataAccess.Data;

public class StreakResetService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StreakResetService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ResetStreaks(stoppingToken);

            // hər 1 saatdan bir yoxla
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ResetStreaks(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var usersToReset = await db.UserStats
            .Where(x =>
                x.LastStreakDateUtc != null &&
                x.LastStreakDateUtc.Value.Date < yesterday &&
                x.CurrentStreak > 0)
            .ToListAsync(ct);

        if (!usersToReset.Any())
            return;

        foreach (var user in usersToReset)
        {
            user.CurrentStreak = 0;
        }

        await db.SaveChangesAsync(ct);
    }
}