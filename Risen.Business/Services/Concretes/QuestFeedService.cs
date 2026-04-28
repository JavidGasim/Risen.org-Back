using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Quests;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class QuestFeedService : IQuestFeedService
    {
        private readonly AppDbContext _db;
        private readonly IQuestEntitlementService _questEnt;

        public QuestFeedService(AppDbContext db, IQuestEntitlementService questEnt)
        {
            _db = db;
            _questEnt = questEnt;
        }

        public async Task<IReadOnlyList<Risen.Contracts.Quests.QuestListItemDto>> GetAllAsync(Guid userId, int limit, int offset, bool includeInactive, CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var q = _db.Quests.AsNoTracking().Include(x => x.Options).AsQueryable();
            if (!includeInactive) q = q.Where(x => x.IsActive);

            var items = await q.OrderByDescending(x => x.CreatedAtUtc).Skip(offset).Take(limit).ToListAsync(ct);
            var questIds = items.Select(x => x.Id).ToList();
            var completedEver = (await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && questIds.Contains(a.QuestId) && a.CompletedDateUtc != null)
                .Select(a => a.QuestId)
                .ToListAsync(ct)).ToHashSet();

            return items.Select(quest => new Risen.Contracts.Quests.QuestListItemDto(
                Id: quest.Id,
                Title: quest.QuestionText,
                Description: quest.Description,
                Difficulty: quest.Difficulty,
                BaseXp: quest.BaseXp,
                IsPremiumOnly: quest.IsPremiumOnly,
                IsCompletedToday: false,
                IsCompletedEver: completedEver.Contains(quest.Id)
            )).ToList();
        }

        public async Task<TodayQuestsResponse> GetTodayAsync(Guid userId, int take, CancellationToken ct)
        {
            take = Math.Clamp(take, 1, 50);

            var today = DateTime.UtcNow.Date;
            var start = today;
            var end = today.AddDays(1);

            // plan policy (isPremium, plan, dailyLimit, advancedAllowed)
            var (isPremium, _, dailyLimit, advancedAllowed) = await _questEnt.GetQuestPolicyAsync(userId, ct);

            // bu gün tamamlanan quest sayı
            var completedTodayCount = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId
                              && a.CompletedDateUtc != null
                              && a.CompletedDateUtc >= start
                              && a.CompletedDateUtc < end, ct);

            var remaining = Math.Max(0, dailyLimit - completedTodayCount);

            // bu gün tamamlanan quest-lərin listi
            var completedQuestIds = await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId
                         && a.CompletedDateUtc != null
                         && a.CompletedDateUtc >= start
                         && a.CompletedDateUtc < end)
                .Select(a => a.QuestId)
                .ToListAsync(ct);

            // eligible quest-lər
            var q = _db.Quests.AsNoTracking()
    .Include(x => x.Options)
    .Where(x => x.IsActive);

            if (!isPremium)
                q = q.Where(x => !x.IsPremiumOnly);

            if (!advancedAllowed)
                q = q.Where(x => x.Difficulty != QuestDifficulty.Advanced);

            var eligible = await q.ToListAsync(ct);


            // “Today rotation” — eyni gün üçün deterministic order
            var dayKey = today.ToString("yyyyMMdd");
            var completedSet = completedQuestIds.ToHashSet();
            var ordered = eligible
                .OrderBy(x => DeterministicScore(x.Id, dayKey))
                .Take(take)
                .ToList();

            // Determine if each quest was ever completed by the user
            var questIds = ordered.Select(q => q.Id).ToList();
            var completedEverSet = (await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && questIds.Contains(a.QuestId) && a.CompletedDateUtc != null)
                .Select(a => a.QuestId)
                .ToListAsync(ct)).ToHashSet();

            var todayDtos = ordered
                .Select(x => new TodayQuestDto(
                    Id: x.Id,
                    Title: x.QuestionText,
                    XpReward: x.BaseXp, // alias varsa BaseXp-ə bağlanır
                    IsCompletedToday: completedSet.Contains(x.Id),
                    IsCompletedEver: completedEverSet.Contains(x.Id),
                    Options: x.Options
                        .OrderBy(o => o.Index)
                        .Select(o => new QuestOptionDto(o.Index, o.Text))
                        .ToList()
                ))
                .ToList();

            return new TodayQuestsResponse(
                DailyLimit: dailyLimit,
                CompletedToday: completedTodayCount,
                RemainingToday: remaining,
                Items: todayDtos
            );
        }

        private static long DeterministicScore(Guid id, string dayKey)
        {
            var bytes = Encoding.UTF8.GetBytes(id.ToString("N") + ":" + dayKey);
            var hash = SHA256.HashData(bytes);
            return BitConverter.ToInt64(hash, 0);
        }
    }
}
