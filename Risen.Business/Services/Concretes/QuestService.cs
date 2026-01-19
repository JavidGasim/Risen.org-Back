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
        private readonly IQuestEntitlementService _ent;

        public QuestService(AppDbContext db, IQuestEntitlementService ent)
        {
            _db = db;
            _ent = ent;
        }

        public async Task<SubmitQuestAnswerResponse> SubmitAsync(Guid userId, Guid questId, int selectedOptionIndex, CancellationToken ct)
        {
            if (selectedOptionIndex < 0 || selectedOptionIndex > 4)
                throw new ArgumentOutOfRangeException(nameof(selectedOptionIndex), "SelectedOptionIndex must be 0..4.");

            // policy
            var (isPremium, _, dailyLimit, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            // quest fetch + policy enforcement
            var questQuery = _db.Quests
                .Include(x => x.Options)
                .Where(x => x.Id == questId && x.IsActive);

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
                throw new KeyNotFoundException("Quest not found or not accessible.");

            // MCQ enforcement
            if (quest.Options.Count != 5)
                throw new InvalidOperationException("Quest must have exactly 5 options.");

            if (quest.CorrectOptionIndex < 0 || quest.CorrectOptionIndex > 4)
                throw new InvalidOperationException("Quest has invalid CorrectOptionIndex.");

            var isCorrect = selectedOptionIndex == quest.CorrectOptionIndex;

            var now = DateTime.UtcNow;
            var today = now.Date;
            var start = today;
            var end = today.AddDays(1);

            // daily limit reached?
            var completedToday = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId
                              && a.CompletedDateUtc != null
                              && a.CompletedDateUtc >= start
                              && a.CompletedDateUtc < end, ct);

            var remaining = Math.Max(0, dailyLimit - completedToday);
            var limitReached = remaining <= 0;

            // XP rule: only first correct attempt gives XP (and only if daily limit not reached)
            var alreadyCorrect = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.QuestId == questId && a.IsCorrect, ct);

            var earnedXp = (!limitReached && isCorrect && !alreadyCorrect) ? quest.BaseXp : 0;

            // Completed only when correct AND limit not reached
            DateTime? completedDate = (!limitReached && isCorrect) ? now : null;

            // log attempt always
            var attempt = new QuestAttempt
            {
                Id = Guid.NewGuid(),
                QuestId = questId,
                UserId = userId,
                SelectedOptionIndex = selectedOptionIndex,
                IsCorrect = isCorrect,
                EarnedXp = earnedXp,
                AnsweredAtUtc = now,
                CompletedDateUtc = completedDate
            };

            _db.QuestAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);

            return new SubmitQuestAnswerResponse(
                IsCorrect: isCorrect,
                EarnedXp: earnedXp,
                CorrectOptionIndex: quest.CorrectOptionIndex, // istəmirsənsə null qaytar
                DailyLimitReached: limitReached
            );
        }
    }
}
