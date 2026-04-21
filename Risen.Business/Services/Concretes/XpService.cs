using Microsoft.EntityFrameworkCore;
using Risen.Business.Exceptions;
using Risen.Business.Services.Abstracts;
using Risen.Business.Utils;
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
        private readonly IStatsService _statsService;

        public XpService(AppDbContext db, IStatsService statsService)
        {
            _db = db;
            _statsService = statsService;
        }

        public async Task<AwardXpResponse> AwardAsync(Guid actorId, AwardXpRequest req, CancellationToken ct, bool commit = true)
        {
            if (actorId == Guid.Empty) throw new BadRequestException("Invalid actor id.");
            if (req is null) throw new InvalidOperationException("Request is null.");
            if (string.IsNullOrWhiteSpace(req.SourceKey)) throw new InvalidOperationException("SourceKey is required.");
            if (req.BaseXp <= 0) throw new BadRequestException("BaseXp must be > 0.");


            var sourceKey = req.SourceKey.Trim();

            // 1) Idempotency check
            var targetUserId = req.TargetUserId ?? actorId;

            var existingTxn = await _db.XpTransactions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == targetUserId && x.SourceType == req.SourceType && x.SourceKey == sourceKey, ct);

            if (existingTxn is not null)
            {
                var existingStats = await _statsService.EnsureStatsAsync(targetUserId, ct);

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
            var stats = await _statsService.EnsureStatsAsync(targetUserId, ct);

            // 4) Transaction yaz
            _db.XpTransactions.Add(new XpTransaction
            {
                Id = Guid.NewGuid(),
                UserId = targetUserId,
                SourceType = req.SourceType,
                SourceKey = sourceKey,
                BaseXp = req.BaseXp,
                DifficultyMultiplier = multiplier,
                FinalXp = finalXp,
                CreatedAtUtc = DateTime.UtcNow
            });
            // If this award is performed by an admin (actor != target) record admin audit info
            if (actorId != targetUserId)
            {
                var added = _db.ChangeTracker.Entries<XpTransaction>()
                    .FirstOrDefault(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added && e.Entity.SourceKey == sourceKey);
                if (added != null)
                {
                    added.Entity.AdminId = actorId;
                    // if admin provided a reason in the request, persist it
                    added.Entity.AdminReason = req.AdminReason;
                }

                // also add AdminAction audit record
                _db.AdminActions.Add(new Risen.Entities.Entities.AdminAction
                {
                    Id = Guid.NewGuid(),
                    AdminId = actorId,
                    TargetUserId = targetUserId,
                    ActionType = "AwardXp",
                    Details = $"SourceType={req.SourceType}; SourceKey={sourceKey}; BaseXp={req.BaseXp}; Mult={req.DifficultyMultiplier}; Reason={req.AdminReason}",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

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
                    UserId = targetUserId,
                    FromTierId = stats.CurrentLeagueTierId,
                    ToTierId = newTier.Id,
                    TotalXpAtChange = stats.TotalXp,
                    ChangedAtUtc = DateTime.UtcNow
                });

                stats.CurrentLeagueTierId = newTier.Id;
                stats.UpdatedAtUtc = DateTime.UtcNow;
            }

            // Risen Score — tier dəyişsə də dəyişməsə də yenilə
            stats.RisenScore = RisenScoreCalculator.Calculate(
                newTier.Weight,
                stats.TotalXp,
                stats.CurrentStreak
            );

            try
            {
                if (commit)
                    await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // likely unique constraint violation (concurrent award). Read the existing transaction and stats and return idempotent result.
                var existingTxn2 = await _db.XpTransactions.AsNoTracking()
                    .Where(x => x.UserId == targetUserId && x.SourceType == req.SourceType && x.SourceKey == sourceKey)
                    .FirstOrDefaultAsync(ct);

                var stats2 = await _statsService.EnsureStatsAsync(targetUserId, ct);

                var tier2 = await _db.LeagueTiers.AsNoTracking()
                    .Where(t => t.Id == stats2.CurrentLeagueTierId)
                    .Select(t => t.Code)
                    .FirstAsync(ct);

                return new AwardXpResponse(
                    FinalXp: existingTxn2 != null ? existingTxn2.FinalXp : finalXp,
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

        public async Task<AwardXpResponse> RevokeAsync(Guid userId, Risen.Contracts.Gamification.RevokeXpRequest req, CancellationToken ct)
        {
            if (userId == Guid.Empty) throw new BadRequestException("Invalid user id.");
            if (req is null) throw new InvalidOperationException("Request is null.");
            if (string.IsNullOrWhiteSpace(req.OriginalSourceKey)) throw new InvalidOperationException("OriginalSourceKey is required.");

            if (req.TargetUserId == Guid.Empty) throw new BadRequestException("TargetUserId is required.");

            var targetUserId = req.TargetUserId;
            var origKey = req.OriginalSourceKey.Trim();

            var originalTxn = await _db.XpTransactions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == targetUserId && x.SourceKey == origKey, ct);

            if (originalTxn is null)
                throw new NotFoundException("Original XP transaction not found for target user.");

            // create compensating transaction
            var negativeXp = -originalTxn.FinalXp;

            var revokeSourceKey = $"Revoke:{originalTxn.Id}";

            // avoid double-revoke
            var alreadyRevoked = await _db.XpTransactions.AsNoTracking()
                .AnyAsync(x => x.SourceKey == revokeSourceKey, ct);
            if (alreadyRevoked)
                throw new InvalidOperationException("This transaction has already been revoked.");

            _db.XpTransactions.Add(new XpTransaction
            {
                Id = Guid.NewGuid(),
                UserId = targetUserId,
                SourceType = Risen.Entities.Entities.XpSourceType.AdminAdjustment,
                SourceKey = revokeSourceKey,
                BaseXp = negativeXp,
                DifficultyMultiplier = 1.0m,
                FinalXp = negativeXp,
                CreatedAtUtc = DateTime.UtcNow
            });
            // set admin audit info on the transaction being added via change tracker
            var added = _db.ChangeTracker.Entries<XpTransaction>()
                .FirstOrDefault(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added && e.Entity.SourceKey == revokeSourceKey);
            if (added != null)
            {
                added.Entity.AdminId = userId; // admin performing the revoke
                added.Entity.AdminReason = req.Reason;
            }

            // add admin audit record
            _db.AdminActions.Add(new Risen.Entities.Entities.AdminAction
            {
                Id = Guid.NewGuid(),
                AdminId = userId,
                TargetUserId = targetUserId,
                ActionType = "RevokeXp",
                Details = $"OriginalTxn={originalTxn.Id}; OrigSourceKey={originalTxn.SourceKey}; OrigFinalXp={originalTxn.FinalXp}; Reason={req.Reason}",
                CreatedAtUtc = DateTime.UtcNow
            });

            var stats = await _statsService.EnsureStatsAsync(targetUserId, ct);

            stats.TotalXp += negativeXp;
            stats.UpdatedAtUtc = DateTime.UtcNow;

            var newTier = await FindTierByXpAsync(stats.TotalXp, ct);
            if (stats.CurrentLeagueTierId != newTier.Id)
            {
                _db.UserLeagueHistories.Add(new UserLeagueHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = targetUserId,
                    FromTierId = stats.CurrentLeagueTierId,
                    ToTierId = newTier.Id,
                    TotalXpAtChange = stats.TotalXp,
                    ChangedAtUtc = DateTime.UtcNow
                });

                stats.CurrentLeagueTierId = newTier.Id;
                stats.UpdatedAtUtc = DateTime.UtcNow;
            }

            stats.RisenScore = RisenScoreCalculator.Calculate(newTier.Weight, stats.TotalXp, stats.CurrentStreak);

            await _db.SaveChangesAsync(ct);

            return new AwardXpResponse(FinalXp: negativeXp, NewTotalXp: stats.TotalXp, NewLeague: newTier.Code.ToString());
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
