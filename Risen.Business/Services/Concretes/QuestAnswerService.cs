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
    public class QuestAnswerService : IQuestAnswerService
    {
        private readonly AppDbContext _db;
        private readonly IXpService _xp;
        private readonly IQuestEntitlementService _ent;
        private readonly QuestPolicyOptions _opt;

        public QuestAnswerService(
            AppDbContext db,
            IXpService xp,
            IQuestEntitlementService ent,
            IOptions<QuestPolicyOptions> opt)
        {
            _db = db;
            _xp = xp;
            _ent = ent;
            _opt = opt.Value;
        }

        public async Task<SubmitQuestAnswerResponse> SubmitAsync(Guid userId, SubmitQuestAnswerRequest req, CancellationToken ct)
        {
            if (req.SelectedIndex < 0 || req.SelectedIndex > 4)
                throw new InvalidOperationException("SelectedIndex must be between 0 and 4.");

            var today = DateTime.UtcNow.Date;
            var start = today;
            var end = today.AddDays(1);

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var quest = await _db.Quests
                .FirstOrDefaultAsync(x => x.Id == req.QuestId && x.IsActive, ct);

            if (quest is null)
                throw new InvalidOperationException("Quest not found.");

            var (isPremium, _, dailyLimit, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            if (!isPremium && quest.IsPremiumOnly)
                throw new InvalidOperationException("This quest is only for Premium.");

            if (!advancedAllowed && quest.Difficulty == QuestDifficulty.Advanced)
                throw new InvalidOperationException("Advanced difficulty is closed for your plan.");

            // daily limit check
            var completedTodayCount = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId
                              && a.CompletedDateUtc != null
                              && a.CompletedDateUtc >= start
                              && a.CompletedDateUtc < end, ct);

            if (completedTodayCount >= dailyLimit)
                throw new InvalidOperationException("Daily quest limit reached.");

            // same quest/day check
            var already = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId
                            && a.QuestId == quest.Id
                            && a.CompletedDateUtc != null
                            && a.CompletedDateUtc >= start
                            && a.CompletedDateUtc < end, ct);

            if (already)
                throw new InvalidOperationException("This quest has already been submitted today.");

            // options: selected + correct
            var options = await _db.QuestOptions.AsNoTracking()
                .Where(o => o.QuestId == quest.Id)
                .OrderBy(o => o.Index)
                .Select(o => new { o.Id, o.Index, o.IsCorrect })
                .ToListAsync(ct);

            if (options.Count != 5)
                throw new InvalidOperationException("Quest must have exactly 5 options.");

            var correct = options.FirstOrDefault(o => o.IsCorrect);
            if (correct is null)
                throw new InvalidOperationException("Quest correct option is not configured.");

            var selected = options.FirstOrDefault(o => o.Index == req.SelectedIndex);
            if (selected is null)
                throw new InvalidOperationException("Invalid SelectedIndex for this quest.");

            var isCorrectAnswer = selected.Id == correct.Id;

            // Stats ensure
            var stats = await EnsureStatsAsync(userId, ct);

            // streak bonus (gündə 1 dəfə) — aktivlik kimi hesablayırıq
            if (stats.LastStreakDateUtc != today)
            {
                var streakKey = $"Streak:{today:yyyyMMdd}";

                await _xp.AwardAsync(userId, new AwardXpRequest(
                    SourceType: XpSourceType.StreakBonus,
                    SourceKey: streakKey,
                    BaseXp: _opt.StreakBonusXp,
                    DifficultyMultiplier: 1.0m
                ), ct);

                var yesterday = today.AddDays(-1);

                stats.CurrentStreak = (stats.LastStreakDateUtc == yesterday)
                    ? stats.CurrentStreak + 1
                    : 1;

                if (stats.CurrentStreak > stats.LongestStreak)
                    stats.LongestStreak = stats.CurrentStreak;

                stats.LastStreakDateUtc = today;
                stats.UpdatedAtUtc = DateTime.UtcNow;
            }

            // Quest XP yalnız doğru cavaba
            int awardedXp = 0;
            if (isCorrectAnswer)
            {
                var multiplier = quest.Difficulty == QuestDifficulty.Advanced
                    ? _opt.AdvancedMultiplier
                    : _opt.NormalMultiplier;

                var questKey = $"Quest:{quest.Id}:date:{today:yyyyMMdd}";

                var xpRes = await _xp.AwardAsync(userId, new AwardXpRequest(
                    SourceType: XpSourceType.QuestCompletion,
                    SourceKey: questKey,
                    BaseXp: quest.BaseXp,
                    DifficultyMultiplier: multiplier
                ), ct);

                awardedXp = xpRes.FinalXp;
            }

            // attempt write
            _db.QuestAttempts.Add(new QuestAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                QuestId = quest.Id,
                CompletedAtUtc = DateTime.UtcNow,
                CompletedDateUtc = today,
                AwardedXp = awardedXp,
                SelectedOptionId = selected.Id,
                IsCorrect = isCorrectAnswer
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            // total xp + league read
            var totalXp = await _db.UserStats.AsNoTracking()
                .Where(s => s.UserId == userId)
                .Select(s => s.TotalXp)
                .FirstAsync(ct);

            var league = await (
                from s in _db.UserStats.AsNoTracking()
                join t in _db.LeagueTiers.AsNoTracking() on s.CurrentLeagueTierId equals t.Id
                where s.UserId == userId
                select t.Code.ToString()
            ).FirstAsync(ct);

            return new SubmitQuestAnswerResponse(
                IsCorrect: isCorrectAnswer,
                CorrectIndex: correct.Index,
                AwardedXp: awardedXp,
                TotalXp: totalXp,
                League: league,
                CurrentStreak: stats.CurrentStreak,
                LongestStreak: stats.LongestStreak
            );
        }

        private async Task<UserStats> EnsureStatsAsync(Guid userId, CancellationToken ct)
        {
            var stats = await _db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (stats is not null) return stats;

            var rookieId = await _db.LeagueTiers.AsNoTracking()
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
