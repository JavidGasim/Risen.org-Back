using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Risen.Business.Exceptions;
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
        private readonly IQuestEntitlementService _ent;
        private readonly IXpService _xp;
        private readonly QuestPolicyOptions _opt;
        private readonly IStatsService _statsService;

        public QuestService(
            AppDbContext db,
            IQuestEntitlementService ent,
            IXpService xp,
            IOptions<QuestPolicyOptions> opt,
            IStatsService statsService)
        {
            _db = db;
            _ent = ent;
            _xp = xp;
            _opt = opt.Value;
            _statsService = statsService;
        }

        public async Task<SubmitQuestAnswerResponse> SubmitAsync(Guid userId, SubmitQuestAnswerRequest req, CancellationToken ct)
        {
            if (req.SelectedIndex < 0 || req.SelectedIndex > 4)
                throw new BadRequestException("SelectedIndex must be 0..4.");

            var now = DateTime.UtcNow;
            var today = now.Date;
            var start = today;
            var end = today.AddDays(1);

            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            var (isPremium, _, dailyLimit, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            var questQuery = _db.Quests
                .Include(x => x.Options)
                .Where(x => x.Id == req.QuestId && x.IsActive);

            if (!isPremium)
            {
                questQuery = questQuery.Where(x => !x.IsPremiumOnly);
                if (!advancedAllowed)
                    questQuery = questQuery.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }
            else
            {
                if (!advancedAllowed)
                    questQuery = questQuery.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }

            var quest = await questQuery.FirstOrDefaultAsync(ct);
            if (quest is null)
                throw new NotFoundException("Quest not found or not accessible.");

            if (quest.Options is null || quest.Options.Count != 5)
                throw new BadRequestException("Quest must have exactly 5 options.");

            if (quest.CorrectOptionIndex < 0 || quest.CorrectOptionIndex > 4)
                throw new InvalidOperationException("Quest has invalid CorrectOptionIndex.");

            var selectedOption = quest.Options.FirstOrDefault(o => o.Index == req.SelectedIndex);
            if (selectedOption is null)
                throw new InvalidOperationException("Selected option not found.");

            var isCorrect = req.SelectedIndex == quest.CorrectOptionIndex;

            var completedTodayCount = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId
                              && a.CompletedDateUtc != null
                              && a.CompletedDateUtc >= start
                              && a.CompletedDateUtc < end, ct);

            var limitReached = completedTodayCount >= dailyLimit;

            var alreadyCompletedEver = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId
                            && a.QuestId == req.QuestId
                            && a.CompletedDateUtc != null, ct);

            var multiplier = quest.Difficulty switch
            {
                QuestDifficulty.Advanced => _opt.AdvancedMultiplier,
                QuestDifficulty.Intermediate => _opt.IntermediateMultiplier,
                _ => _opt.NormalMultiplier
            };

            var stats = await _statsService.EnsureStatsAsync(userId, ct);

            AwardXpResponse? lastXpRes = null;
            var gainedThisSubmit = 0;

            // ✅ QUEST XP
            if (!limitReached && isCorrect && !alreadyCompletedEver)
            {
                var questSourceKey = $"Quest:{quest.Id}:date:{today:yyyyMMdd}";

                var questXp = await _xp.AwardAsync(
                    userId,
                    new AwardXpRequest(
                        SourceType: XpSourceType.QuestCompletion,
                        SourceKey: questSourceKey,
                        BaseXp: quest.BaseXp,
                        DifficultyMultiplier: multiplier
                    ),
                    ct);

                lastXpRes = questXp;
                gainedThisSubmit += questXp.FinalXp;

                // ✅ STREAK bonus XP (ONLY XP part qalır)
                if (stats.LastStreakDateUtc != today)
                {
                    var streakSourceKey = $"Streak:{today:yyyyMMdd}";

                    var streakXp = await _xp.AwardAsync(
                        userId,
                        new AwardXpRequest(
                            SourceType: XpSourceType.StreakBonus,
                            SourceKey: streakSourceKey,
                            BaseXp: _opt.StreakBonusXp,
                            DifficultyMultiplier: 1.0m
                        ),
                        ct);

                    gainedThisSubmit += streakXp.FinalXp;

                    // 🔥 ONLY update streak via StatsService
                    await _statsService.UpdateStreakAsync(stats, today, ct);
                }
            }

            DateTime? completedDateUtc = (!limitReached && !alreadyCompletedEver) ? now : null;

            var attempt = new QuestAttempt
            {
                Id = Guid.NewGuid(),
                QuestId = req.QuestId,
                UserId = userId,
                SelectedOptionId = selectedOption.Id,
                IsCorrect = isCorrect,
                AwardedXp = gainedThisSubmit,
                CompletedAtUtc = now,
                CompletedDateUtc = completedDateUtc
            };

            if (!alreadyCompletedEver)
                _db.QuestAttempts.Add(attempt);

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            long totalXp;
            string league;

            if (lastXpRes is not null)
            {
                totalXp = lastXpRes.NewTotalXp;
                league = lastXpRes.NewLeague;
            }
            else
            {
                totalXp = stats.TotalXp;
                league = await _db.LeagueTiers.AsNoTracking()
                    .Where(t => t.Id == stats.CurrentLeagueTierId)
                    .Select(t => t.Code.ToString())
                    .FirstAsync(ct);
            }

            return new SubmitQuestAnswerResponse(
                IsCorrect: isCorrect,
                CorrectIndex: quest.CorrectOptionIndex,
                AwardedXp: gainedThisSubmit,
                TotalXp: totalXp,
                League: league,
                CurrentStreak: stats.CurrentStreak,
                LongestStreak: stats.LongestStreak
            );
        }

    }
}