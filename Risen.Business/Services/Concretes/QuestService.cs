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

        public QuestService(AppDbContext db, IXpService xp, IQuestEntitlementService questEnt, IOptions<QuestPolicyOptions> opt)
        {
            _db = db;
            _xp = xp;
            _questEnt = questEnt;
            _opt = opt.Value;
        }

        public async Task<CompleteQuestResponse> CompleteAsync(Guid userId, CompleteQuestRequest req, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;

            var quest = await _db.Quests.AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == req.QuestId && q.IsActive, ct);

            if (quest is null)
                throw new InvalidOperationException("Quest not found.");

            var (isPremium, plan, dailyLimit, advancedAllowed) =
                await _questEnt.GetQuestPolicyAsync(userId, ct);

            if (quest.IsPremiumOnly && !isPremium)
                throw new InvalidOperationException("This quest is only for Premium.");

            if (quest.Difficulty == QuestDifficulty.Advanced && !advancedAllowed)
                throw new InvalidOperationException("Advanced difficulty is close Free plan.");

            // daily limit check (bu gün neçə quest tamamlayıb)
            var todayCount = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId && a.CompletedDateUtc == today, ct);

            if (todayCount >= dailyLimit)
                throw new InvalidOperationException("Daily quest limit reached.");

            // eyni quest bu gün tamamlanıbsa (fast pre-check)
            var exists = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.QuestId == quest.Id && a.CompletedDateUtc == today, ct);

            if (exists)
                throw new InvalidOperationException("This quest has already been completed today..");

            // server-side multiplier
            var multiplier = quest.Difficulty switch
            {
                QuestDifficulty.Advanced => _opt.AdvancedMultiplier,
                _ => _opt.NormalMultiplier
            };

            // SourceKey server tərəfdə (cheat olmasın)
            var sourceKey = $"Quest:{quest.Id}:date:{today:yyyyMMdd}";

            // XP claim (XpService özü idempotentdir)
            var xpRes = await _xp.ClaimAsync(userId, new ClaimXpRequest(sourceKey, quest.BaseXp, multiplier), ct);

            // Stats təmin et (bəzən register zamanı stats yazılmayıbsa)
            var stats = await EnsureStatsAsync(userId, ct);

            // attempt yaz + streak update
            _db.QuestAttempts.Add(new QuestAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                QuestId = quest.Id,
                CompletedAtUtc = DateTime.UtcNow,
                CompletedDateUtc = today,
                AwardedXp = xpRes.AwardedXp
            });

            // streak
            if (stats.LastStreakDateUtc != today)
            {
                var yesterday = today.AddDays(-1);

                stats.CurrentStreak = (stats.LastStreakDateUtc == yesterday)
                    ? stats.CurrentStreak + 1
                    : 1;

                if (stats.CurrentStreak > stats.LongestStreak)
                    stats.LongestStreak = stats.CurrentStreak;

                stats.LastStreakDateUtc = today;
                stats.UpdatedAtUtc = DateTime.UtcNow;
            }

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Unikal index (UserId, QuestId, CompletedDateUtc) paralel request-də partlaya bilər
                throw new InvalidOperationException("Bu quest bu gün artıq tamamlanıb.");
            }

            return new CompleteQuestResponse(
                xpRes.AwardedXp,
                xpRes.TotalXp,
                xpRes.League,
                stats.CurrentStreak,
                stats.LongestStreak
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
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.UserStats.Add(stats);
            await _db.SaveChangesAsync(ct);

            return stats;
        }
    }
}
