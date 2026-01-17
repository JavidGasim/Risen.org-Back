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

        public QuestService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<QuestDto> GetQuestAsync(Guid questId, CancellationToken ct)
        {
            var quest = await _db.Quests
                .Where(q => q.Id == questId && q.IsActive)
                .Select(q => new QuestDto(
                    q.Id,
                    q.QuestionText,
                    q.Options.OrderBy(o => o.Index)
                             .Select(o => new QuestOptionDto(o.Index, o.Text))
                             .ToList(),
                    q.BaseXp
                ))
                .FirstOrDefaultAsync(ct);

            if (quest is null) throw new KeyNotFoundException("Quest not found.");
            return quest;
        }

        public async Task<SubmitQuestAnswerResponse> SubmitAnswerAsync(
            Guid questId,
            Guid userId,
            int selectedIndex,
            CancellationToken ct)
        {
            if (selectedIndex < 0 || selectedIndex > 4)
                throw new ArgumentOutOfRangeException(nameof(selectedIndex), "SelectedOptionIndex must be 0..4.");

            var quest = await _db.Quests
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questId && q.IsActive, ct);

            if (quest is null) throw new KeyNotFoundException("Quest not found.");

            // sərt qayda: 5 variant olmalıdır
            if (quest.Options.Count != 5)
                throw new InvalidOperationException("Quest must have exactly 5 options.");

            if (quest.CorrectOptionIndex < 0 || quest.CorrectOptionIndex > 4)
                throw new InvalidOperationException("Quest has invalid CorrectOptionIndex.");

            var isCorrect = selectedIndex == quest.CorrectOptionIndex;
            var earnedXp = isCorrect ? quest.BaseXp : 0;

            var attempt = new QuestAttempt
            {
                Id = Guid.NewGuid(),
                QuestId = questId,
                UserId = userId,
                SelectedOptionIndex = selectedIndex,
                IsCorrect = isCorrect,
                EarnedXp = earnedXp,
                AnsweredAtUtc = DateTime.UtcNow
            };

            _db.QuestAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);

            return new SubmitQuestAnswerResponse(
                IsCorrect: isCorrect,
                CorrectOptionIndex: quest.CorrectOptionIndex, // istəmirsinizsə null edin
                EarnedXp: earnedXp
            );
        }
    }
}
