using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Gamification;
using Risen.Contracts.Xp;
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

        public async Task<AwardXpResponse> AwardAsync(Guid userId, AwardXpRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.SourceKey))
                throw new InvalidOperationException("SourceKey is required.");

            if (req.BaseXp <= 0)
                throw new InvalidOperationException("BaseXp must be > 0.");

            if (req.DifficultyMultiplier <= 0)
                throw new InvalidOperationException("DifficultyMultiplier must be > 0.");

            // 1) Stats
            var stats = await _db.UserStats.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (stats is null)
                throw new InvalidOperationException("UserStats not found. Ensure it is created on registration.");

            // 2) Old tier
            var oldTier = await _db.LeagueTiers.AsNoTracking()
                .FirstAsync(t => t.Id == stats.CurrentLeagueTierId, ct);

            // 3) FinalXp hesabla (int)
            var finalXp = (int)Math.Round(req.BaseXp * req.DifficultyMultiplier, 0, MidpointRounding.AwayFromZero);
            if (finalXp <= 0) finalXp = 1;

            // 4) Transaction (idempotent)
            var tx = new XpTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SourceType = req.SourceType,
                SourceKey = req.SourceKey.Trim(),
                BaseXp = req.BaseXp,
                DifficultyMultiplier = req.DifficultyMultiplier,
                FinalXp = finalXp,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.XpTransactions.Add(tx);

            // 5) Stats update
            stats.TotalXp += finalXp;
            stats.UpdatedAtUtc = DateTime.UtcNow;

            // 6) League promotion
            var newTier = await ResolveTierAsync(stats.TotalXp, ct);
            if (newTier.Id != stats.CurrentLeagueTierId)
                stats.CurrentLeagueTierId = newTier.Id;

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // eyni SourceKey-lə ikinci dəfə award
                throw new InvalidOperationException("XP already awarded for this source (idempotency hit).");
            }

            return new AwardXpResponse(
                TransactionId: tx.Id,
                FinalXp: finalXp,
                NewTotalXp: stats.TotalXp,
                OldLeague: oldTier.Code.ToString(),
                NewLeague: newTier.Code.ToString()
            );
        }

        private async Task<LeagueTier> ResolveTierAsync(long totalXp, CancellationToken ct)
        {
            // MinXp <= xp && (MaxXp null || xp <= MaxXp)
            return await _db.LeagueTiers.AsNoTracking()
                .Where(t => t.MinXp <= totalXp && (t.MaxXp == null || totalXp <= t.MaxXp.Value))
                .OrderByDescending(t => t.SortOrder)
                .FirstAsync(ct);
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            // SqlServer unique violation: 2601 / 2627
            if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql)
                return sql.Number is 2601 or 2627;

            return false;
        }
    }
}
