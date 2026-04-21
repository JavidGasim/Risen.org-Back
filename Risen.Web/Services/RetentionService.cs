using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Risen.Business.Options;
using Risen.DataAccess.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Risen.Web.Services
{
    public class RetentionService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly RetentionOptions _opt;
        private readonly ILogger<RetentionService> _logger;

        public RetentionService(IServiceProvider sp, IOptions<RetentionOptions> opt, ILogger<RetentionService> logger)
        {
            _sp = sp;
            _opt = opt.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RetentionService started. TransactionRetentionDays={Days}, IntervalMinutes={Interval}", _opt.TransactionRetentionDays, _opt.IntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var threshold = DateTime.UtcNow.AddDays(-_opt.TransactionRetentionDays);

                    int moved = 0;
                    do
                    {
                        var old = await db.XpTransactions
                            .Where(x => x.CreatedAtUtc < threshold)
                            .OrderBy(x => x.CreatedAtUtc)
                            .Take(_opt.BatchSize)
                            .ToListAsync(stoppingToken);

                        if (old.Count == 0) break;

                        // map to archive entries
                        var archives = old.Select(x => new Risen.Entities.Entities.XpTransactionArchive
                        {
                            Id = x.Id,
                            UserId = x.UserId,
                            SourceType = x.SourceType,
                            SourceKey = x.SourceKey,
                            BaseXp = x.BaseXp,
                            DifficultyMultiplier = x.DifficultyMultiplier,
                            FinalXp = x.FinalXp,
                            CreatedAtUtc = x.CreatedAtUtc,
                            AdminId = x.AdminId,
                            AdminReason = x.AdminReason,
                            ArchivedAtUtc = DateTime.UtcNow
                        }).ToList();

                        // add archives and remove originals in a transaction
                        await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);
                        try
                        {
                            db.XpTransactionArchives.AddRange(archives);
                            db.XpTransactions.RemoveRange(old);
                            moved += old.Count;
                            await db.SaveChangesAsync(stoppingToken);
                            await tx.CommitAsync(stoppingToken);
                        }
                        catch
                        {
                            await tx.RollbackAsync(stoppingToken);
                            throw;
                        }
                    }
                    while (moved % _opt.BatchSize == 0);

                    if (moved > 0)
                        _logger.LogInformation("RetentionService archived {Count} old XpTransactions older than {Threshold}", moved, threshold);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RetentionService encountered an error");
                }

                await Task.Delay(TimeSpan.FromMinutes(_opt.IntervalMinutes), stoppingToken);
            }
        }
    }
}
