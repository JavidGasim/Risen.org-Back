using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
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
    public class QuestQueryService : IQuestQueryService
    {
        private readonly AppDbContext _db;
        private readonly IQuestEntitlementService _ent;

        public QuestQueryService(AppDbContext db, IQuestEntitlementService ent)
        {
            _db = db;
            _ent = ent;
        }

        public async Task<TodayQuestsResponse> GetTodayAsync(Guid userId, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;

            var (_, _, dailyLimit, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            // bugün tamamlanan questId-lər
            var completedIds = await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && a.CompletedDateUtc == today)
                .Select(a => a.QuestId)
                .ToListAsync(ct);

            var completedSet = completedIds.ToHashSet();

            // quest-ləri policy-yə görə filtr edirik
            // PremiumOnly filtrini ent service-də qaytardığın IsPremium ilə ayrıca etmək də olar,
            // amma biz burada DB filtrində ən sadə formada edirik:
            var (isPremium, _, _, _) = await _ent.GetQuestPolicyAsync(userId, ct);

            var q = _db.Quests.AsNoTracking()
                .Where(x => x.IsActive);

            if (!isPremium)
            {
                q = q.Where(x => !x.IsPremiumOnly);
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }

            var items = await q
                .OrderBy(x => x.Difficulty)
                .ThenBy(x => x.Title)
                .Select(x => new QuestListItemDto(
                    x.Id,
                    x.Title,
                    x.Description,
                    x.Difficulty,
                    x.BaseXp,
                    x.IsPremiumOnly,
                    completedSet.Contains(x.Id)
                ))
                .ToListAsync(ct);

            var completedToday = completedSet.Count;
            var remaining = Math.Max(0, dailyLimit - completedToday);

            return new TodayQuestsResponse(
    DailyLimit: dailyLimit,
    CompletedToday: completedToday,
    RemainingToday: remaining,
    Items: items
);


        }

        public async Task<QuestListItemDto?> GetByIdAsync(Guid userId, Guid questId, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;
            var (isPremium, _, _, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            var completed = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId && a.QuestId == questId && a.CompletedDateUtc == today, ct);

            var q = _db.Quests.AsNoTracking()
                .Where(x => x.Id == questId && x.IsActive);

            if (!isPremium)
            {
                q = q.Where(x => !x.IsPremiumOnly);
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }

            return await q.Select(x => new QuestListItemDto(
                    x.Id,
                    x.Title,
                    x.Description,
                    x.Difficulty,
                    x.BaseXp,
                    x.IsPremiumOnly,
                    completed
                ))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<QuestListItemDto>> GetCatalogAsync(Guid userId, CancellationToken ct)
        {
            var today = DateTime.UtcNow.Date;
            var (isPremium, _, _, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            var completedIds = await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && a.CompletedDateUtc == today)
                .Select(a => a.QuestId)
                .ToListAsync(ct);

            var completedSet = completedIds.ToHashSet();

            var q = _db.Quests.AsNoTracking().Where(x => x.IsActive);

            if (!isPremium)
            {
                q = q.Where(x => !x.IsPremiumOnly);
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }

            return await q
                .OrderBy(x => x.Title)
                .Select(x => new QuestListItemDto(
                    x.Id,
                    x.Title,
                    x.Description,
                    x.Difficulty,
                    x.BaseXp,
                    x.IsPremiumOnly,
                    completedSet.Contains(x.Id)
                ))
                .ToListAsync(ct);
        }
    }
}
