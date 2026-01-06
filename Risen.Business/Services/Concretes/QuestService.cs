using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Risen.Business.Options;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Gamification;
using Risen.Contracts.Quests;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class QuestService : IQuestService
    {
        private readonly AppDbContext _db;
        private readonly IXpService _xp;
        private readonly IQuestEntitlementService _questEnt;
        private readonly QuestPolicyOptions _opt;

        public QuestService(
            AppDbContext db,
            IXpService xp,
            IQuestEntitlementService questEnt,
            IOptions<QuestPolicyOptions> opt)
        {
            _db = db;
            _xp = xp;
            _questEnt = questEnt;
            _opt = opt.Value;
        }

        public async Task<CompleteQuestResponse> CompleteAsync(Guid userId, CompleteQuestRequest req, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var quest = await _db.Quests.AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == req.QuestId && q.IsActive, ct);

            if (quest is null)
                throw new InvalidOperationException("Quest not found.");

            var (isPremium, plan, dailyLimit, advancedAllowed) =
                await _questEnt.GetQuestPolicyAsync(userId, ct);

            if (quest.IsPremiumOnly && !isPremium)
                throw new InvalidOperationException("This quest is only for Premium.");

            if (quest.Difficulty == QuestDifficulty.Advanced && !advancedAllowed)
                throw new InvalidOperationException("Advanced difficulty is closed for Free plan.");

            // Stats olsun (seed user-lər üçün də)
            var stats = await EnsureStatsAsync(userId, ct);

            // daily limit
            var todayCount = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId && a.CompletedDateUtc == today, ct);

            if (todayCount >= dailyLimit)
                throw new InvalidOperationException("Daily quest limit reached.");

            // same quest/day
            var exists = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.QuestId == quest.Id && a.CompletedDateUtc == today, ct);

            if (exists)
                throw new InvalidOperationException("This quest has already been completed today.");

            // multiplier
            var multiplier = quest.Difficulty switch
            {
                QuestDifficulty.Advanced => _opt.AdvancedMultiplier,
                _ => _opt.NormalMultiplier
            };

            // 1) Quest XP (idempotent)
            var questSourceKey = $"Quest:{quest.Id}:date:{today:yyyyMMdd}";
            var questXp = await _xp.AwardAsync(
                userId,
                new AwardXpRequest(
                    SourceType: XpSourceType.QuestCompletion,
                    SourceKey: questSourceKey,
                    BaseXp: quest.BaseXp,
                    DifficultyMultiplier: multiplier
                ),
                ct
            );

            // 2) Streak bonus (gündə 1 dəfə)
            if (stats.LastStreakDateUtc != today)
            {
                var streakKey = $"Streak:{today:yyyyMMdd}";

                await _xp.AwardAsync(
                    userId,
                    new AwardXpRequest(
                        SourceType: XpSourceType.StreakBonus,
                        SourceKey: streakKey,
                        BaseXp: _opt.StreakBonusXp,
                        DifficultyMultiplier: 1.0m
                    ),
                    ct
                );

                var yesterday = today.AddDays(-1);

                stats.CurrentStreak = (stats.LastStreakDateUtc == yesterday)
                    ? stats.CurrentStreak + 1
                    : 1;

                if (stats.CurrentStreak > stats.LongestStreak)
                    stats.LongestStreak = stats.CurrentStreak;

                stats.LastStreakDateUtc = today;
                stats.UpdatedAtUtc = DateTime.UtcNow;
            }

            // 3) attempt write
            _db.QuestAttempts.Add(new QuestAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                QuestId = quest.Id,
                CompletedAtUtc = DateTime.UtcNow,
                CompletedDateUtc = today,
                AwardedXp = questXp.FinalXp
            });

            try
            {
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync(ct);
                throw new InvalidOperationException("This quest has already been completed today.");
            }

            return new CompleteQuestResponse(
                AwardedXp: questXp.FinalXp,
                TotalXp: questXp.NewTotalXp,
                League: questXp.NewLeague,
                CurrentStreak: stats.CurrentStreak,
                LongestStreak: stats.LongestStreak
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

    }
}
