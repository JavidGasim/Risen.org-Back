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

        public XpService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<AwardXpResponse> AwardAsync(Guid userId, AwardXpRequest req, CancellationToken ct)
        {
            if (userId == Guid.Empty) throw new InvalidOperationException("Invalid user id.");
            if (req is null) throw new InvalidOperationException("Request is null.");
            if (string.IsNullOrWhiteSpace(req.SourceKey)) throw new InvalidOperationException("SourceKey is required.");
            if (req.BaseXp <= 0) throw new InvalidOperationException("BaseXp must be > 0.");

            var sourceKey = req.SourceKey.Trim();

            // 1) Idempotency check
            var existingTxn = await _db.XpTransactions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.SourceKey == sourceKey, ct);

            if (existingTxn is not null)
            {
                var existingStats = await EnsureStatsAsync(userId, ct);

                var tier = await _db.LeagueTiers.AsNoTracking()
                    .Where(t => t.Id == existingStats.CurrentLeagueTierId)
                    .Select(t => t.Code)
                    .FirstAsync(ct);

                return new AwardXpResponse(
                    FinalXp: existingTxn.FinalXp,
                    NewTotalXp: existingStats.TotalXp,
                    NewLeague: tier.ToString()
                );
            }

            // 2) Final XP hesabla
            var multiplier = req.DifficultyMultiplier <= 0 ? 1.0m : req.DifficultyMultiplier;
            if (multiplier > 10m) multiplier = 10m;

            var finalXpDecimal = req.BaseXp * multiplier;
            var finalXp = (int)Math.Round(finalXpDecimal, MidpointRounding.AwayFromZero);
            if (finalXp < 1) finalXp = 1;

            // 3) Stats (yoxdursa yarat)
            var stats = await EnsureStatsAsync(userId, ct);

            // 4) Transaction yaz
            _db.XpTransactions.Add(new XpTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SourceType = req.SourceType,
                SourceKey = sourceKey,
                BaseXp = req.BaseXp,
                DifficultyMultiplier = multiplier,
                FinalXp = finalXp,
                CreatedAtUtc = DateTime.UtcNow
            });

            // 5) TotalXp artır
            stats.TotalXp += finalXp;
            stats.UpdatedAtUtc = DateTime.UtcNow;

            // 6) League tier hesabla
            var newTier = await FindTierByXpAsync(stats.TotalXp, ct);
            if (stats.CurrentLeagueTierId != newTier.Id)
            {
                _db.UserLeagueHistories.Add(new UserLeagueHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FromTierId = stats.CurrentLeagueTierId,
                    ToTierId = newTier.Id,
                    TotalXpAtChange = stats.TotalXp,
                    ChangedAtUtc = DateTime.UtcNow
                });

                stats.CurrentLeagueTierId = newTier.Id;
                stats.UpdatedAtUtc = DateTime.UtcNow;
            }

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                var stats2 = await EnsureStatsAsync(userId, ct);

                var tier2 = await _db.LeagueTiers.AsNoTracking()
                    .Where(t => t.Id == stats2.CurrentLeagueTierId)
                    .Select(t => t.Code)
                    .FirstAsync(ct);

                return new AwardXpResponse(
                    FinalXp: finalXp,
                    NewTotalXp: stats2.TotalXp,
                    NewLeague: tier2.ToString()
                );
            }

            return new AwardXpResponse(
                FinalXp: finalXp,
                NewTotalXp: stats.TotalXp,
                NewLeague: newTier.Code.ToString()
            );
        }

        private async Task<UserStats> EnsureStatsAsync(Guid userId, CancellationToken ct)
        {
            var stats = await _db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (stats is not null) return stats;

            var rookieId = await _db.LeagueTiers
                .Where(t => t.Code == LeagueCode.Rookie)
                .Select(t => t.Id)
                .FirstAsync(ct);

            stats = new UserStats
            {
                UserId = userId,
                TotalXp = 0,
                CurrentLeagueTierId = rookieId,
                CurrentStreak = 0,
                LongestStreak = 0,
                LastStreakDateUtc = null,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.UserStats.Add(stats);
            await _db.SaveChangesAsync(ct);

            return stats;
        }

        private async Task<LeagueTier> FindTierByXpAsync(long totalXp, CancellationToken ct)
        {
            var tiers = await _db.LeagueTiers.AsNoTracking()
                .OrderBy(t => t.SortOrder)
                .ToListAsync(ct);

            foreach (var t in tiers)
            {
                var maxOk = !t.MaxXp.HasValue || totalXp <= t.MaxXp.Value;
                if (totalXp >= t.MinXp && maxOk)
                    return t;
            }

            return tiers.First(t => t.Code == LeagueCode.Rookie);
        }
    }
}
