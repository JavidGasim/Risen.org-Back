using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Gamification;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class XpService : IXpService
    {

        private readonly AppDbContext _db;
        public XpService(AppDbContext db) => _db = db;

        public async Task<ClaimXpResponse> ClaimAsync(Guid userId, ClaimXpRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.SourceKey))
                throw new InvalidOperationException("SourceKey is required.");

            if (req.BaseXp <= 0)
                throw new InvalidOperationException("BaseXp must be > 0.");

            var multiplier = req.DifficultyMultiplier <= 0 ? 1m : req.DifficultyMultiplier;
            var finalXp = (int)Math.Round(req.BaseXp * multiplier, MidpointRounding.AwayFromZero);
            if (finalXp < 1) finalXp = 1;

            // idempotency: eyni source üçün artıq claim edilibsə, eyni nəticəni qaytar
            var existingTx = await _db.XpTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.SourceKey == req.SourceKey, ct);

            if (existingTx is not null)
            {
                var stats0 = await EnsureStatsAsync(userId, ct);
                var tier0 = await _db.LeagueTiers.AsNoTracking()
                    .FirstAsync(t => t.Id == stats0.CurrentLeagueTierId, ct);

                return new ClaimXpResponse(existingTx.FinalXp, stats0.TotalXp, tier0.Code.ToString());
            }

            await using var trx = await _db.Database.BeginTransactionAsync(ct);

            // stats ensure
            var stats = await EnsureStatsAsync(userId, ct);

            var oldTierId = stats.CurrentLeagueTierId;

            var tx = new XpTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SourceType = XpSourceType.QuestCompletion,
                SourceKey = req.SourceKey.Trim(),
                BaseXp = req.BaseXp,
                DifficultyMultiplier = multiplier,
                FinalXp = finalXp,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.XpTransactions.Add(tx);

            stats.TotalXp += finalXp;
            stats.UpdatedAtUtc = DateTime.UtcNow;

            var newTier = await FindTierByXpAsync(stats.TotalXp, ct);
            stats.CurrentLeagueTierId = newTier.Id;

            // promotion history
            if (oldTierId != newTier.Id)
            {
                _db.UserLeagueHistories.Add(new UserLeagueHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FromTierId = oldTierId,
                    ToTierId = newTier.Id,
                    TotalXpAtChange = stats.TotalXp,
                    ChangedAtUtc = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);
            await trx.CommitAsync(ct);

            return new ClaimXpResponse(finalXp, stats.TotalXp, newTier.Code.ToString());
        }

        private async Task<UserStats> EnsureStatsAsync(Guid userId, CancellationToken ct)
        {
            var stats = await _db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (stats is not null) return stats;

            var rookie = await _db.LeagueTiers.FirstAsync(t => t.Code == LeagueCode.Rookie, ct);

            stats = new UserStats
            {
                UserId = userId,
                TotalXp = 0,
                CurrentLeagueTierId = rookie.Id,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.UserStats.Add(stats);
            await _db.SaveChangesAsync(ct);
            return stats;
        }

        private async Task<LeagueTier> FindTierByXpAsync(long totalXp, CancellationToken ct)
        {
            // ən yüksək MinXp <= totalXp olan tier
            return await _db.LeagueTiers
                .OrderByDescending(t => t.MinXp)
                .FirstAsync(t => t.MinXp <= totalXp, ct);
        }
    }
}
