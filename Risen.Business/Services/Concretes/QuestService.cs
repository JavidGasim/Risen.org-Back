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

            // policy
            var (isPremium, _, dailyLimit, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            // quest fetch + policy enforcement
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

            // 5 option enforcement
            if (quest.Options is null || quest.Options.Count != 5)
                throw new BadRequestException("Quest must have exactly 5 options.");

            if (quest.CorrectOptionIndex < 0 || quest.CorrectOptionIndex > 4)
                throw new InvalidOperationException("Quest has invalid CorrectOptionIndex.");

            var selectedOption = quest.Options.FirstOrDefault(o => o.Index == req.SelectedIndex);
            if (selectedOption is null)
                throw new InvalidOperationException("Selected option not found.");

            var isCorrect = req.SelectedIndex == quest.CorrectOptionIndex;

            // daily limit: yalnız CompletedDateUtc olanlar sayılır
            var completedTodayCount = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId
                              && a.CompletedDateUtc != null
                              && a.CompletedDateUtc >= start
                              && a.CompletedDateUtc < end, ct);

            var limitReached = completedTodayCount >= dailyLimit;

            // has the user ever completed this quest?
            var alreadyCompletedEver = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId
                            && a.QuestId == req.QuestId
                            && a.CompletedDateUtc != null, ct);

            // difficulty multiplier (server-side)
            var multiplier = quest.Difficulty switch
            {
                QuestDifficulty.Advanced => _opt.AdvancedMultiplier,
                QuestDifficulty.Intermediate => _opt.IntermediateMultiplier,
                _ => _opt.NormalMultiplier
            };

            // XP yalnız: correct + limitReached deyil + bu quest bu gün tamamlanmayıb
            AwardXpResponse? lastXpRes = null;
            var gainedThisSubmit = 0;

            // Stats (streak üçün lazımdır)
            var stats = await _statsService.EnsureStatsAsync(userId, ct);

            if (!limitReached && isCorrect && !alreadyCompletedEver)
            {
                // 1) Quest XP (idempotent SourceKey)
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

                // 2) Streak bonus (gündə 1 dəfə)
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

                    lastXpRes = streakXp;
                    gainedThisSubmit += streakXp.FinalXp;

                    var yesterday = today.AddDays(-1);
                    stats.CurrentStreak = (stats.LastStreakDateUtc == yesterday)
                        ? stats.CurrentStreak + 1
                        : 1;

                    if (stats.CurrentStreak > stats.LongestStreak)
                        stats.LongestStreak = stats.CurrentStreak;

                    stats.LastStreakDateUtc = today;
                    stats.UpdatedAtUtc = DateTime.UtcNow;
                    // Recalculate RisenScore after streak update
                    var tierWeight = await _db.LeagueTiers.AsNoTracking()
                        .Where(t => t.Id == stats.CurrentLeagueTierId)
                        .Select(t => t.Weight)
                        .FirstAsync(ct);

                    stats.RisenScore = Risen.Business.Utils.RisenScoreCalculator.Calculate(
                        tierWeight,
                        stats.TotalXp,
                        stats.CurrentStreak
                    );
                }
            }

            // CompletedDateUtc: record the calendar-day of the attempt when it should count toward
            // the daily limit. Previously this was only set for correct answers; change behavior
            // so that the first qualifying attempt (within limits and not already completed ever)
            // records CompletedDateUtc even if the answer is incorrect.
            // If the quest was already completed ever, do not record a new completion.
            DateTime? completedDateUtc = (!limitReached && !alreadyCompletedEver)
                ? now
                : null;

            // attempt həmişə yazılır (wrong attempt də log olur)
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

            // If the quest was already completed ever, do not insert a new "completed" attempt;
            // still log the attempt (incorrect tries) only if it wasn't already completed ever.
            if (!alreadyCompletedEver)
            {
                _db.QuestAttempts.Add(attempt);
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            // TotalXp + League (XP verilməyibsə də normal qaytar)
            long totalXp;
            string league;

            if (lastXpRes is not null)
            {
                totalXp = lastXpRes.NewTotalXp;
                league = lastXpRes.NewLeague;
            }
            else
            {
                // heç XP verilmədisə: hazır stats-dan oxu
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