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
            var start = today;
            var end = today.AddDays(1);

            var (isPremium, _, dailyLimit, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            // bugün tamamlanan questId-lər
            var completedIds = await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId
                         && a.CompletedDateUtc != null
                         && a.CompletedDateUtc >= start
                         && a.CompletedDateUtc < end)
                .Select(a => a.QuestId)
                .ToListAsync(ct);

            var completedSet = completedIds.ToHashSet();

            var q = _db.Quests.AsNoTracking()
                .Include(x => x.Options)
                .Where(x => x.IsActive);

            if (!isPremium)
            {
                q = q.Where(x => !x.IsPremiumOnly);
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }
            else
            {
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }

            var quests = await q
                .OrderBy(x => x.Difficulty)
                .ThenBy(x => x.Title)
                .ToListAsync(ct);

            var items = quests
                .Select(x => new TodayQuestDto(
                    Id: x.Id,
                    Title: x.Title,
                    XpReward: x.BaseXp,
                    IsCompletedToday: completedSet.Contains(x.Id),
                    Options: x.Options
                        .OrderBy(o => o.Index)
                        .Select(o => new QuestOptionDto(o.Index, o.Text))
                        .ToList()
                ))
                .ToList();

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
            var start = today;
            var end = today.AddDays(1);

            var (isPremium, _, _, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            var completed = await _db.QuestAttempts.AsNoTracking()
                .AnyAsync(a => a.UserId == userId
                            && a.QuestId == questId
                            && a.CompletedDateUtc != null
                            && a.CompletedDateUtc >= start
                            && a.CompletedDateUtc < end, ct);

            var q = _db.Quests.AsNoTracking()
                .Where(x => x.Id == questId && x.IsActive);

            if (!isPremium)
            {
                q = q.Where(x => !x.IsPremiumOnly);
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }
            else
            {
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }

            return await q
                .Select(x => new QuestListItemDto(
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
            var start = today;
            var end = today.AddDays(1);

            var (isPremium, _, _, advancedAllowed) = await _ent.GetQuestPolicyAsync(userId, ct);

            var completedIds = await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId
                         && a.CompletedDateUtc != null
                         && a.CompletedDateUtc >= start
                         && a.CompletedDateUtc < end)
                .Select(a => a.QuestId)
                .ToListAsync(ct);

            var completedSet = completedIds.ToHashSet();

            var q = _db.Quests.AsNoTracking()
                .Where(x => x.IsActive);

            if (!isPremium)
            {
                q = q.Where(x => !x.IsPremiumOnly);
                if (!advancedAllowed)
                    q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);
            }
            else
            {
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
