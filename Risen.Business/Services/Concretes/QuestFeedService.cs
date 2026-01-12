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

        public async Task<TodayQuestsResponse> GetTodayAsync(Guid userId, int take, CancellationToken ct)
        {
            take = Math.Clamp(take, 1, 50);

            var today = DateTime.UtcNow.Date;

            // plan policy (sənin wrapper-in: isPremium, plan, dailyLimit, advancedAllowed)
            var (_, _, dailyLimit, advancedAllowed) = await _questEnt.GetQuestPolicyAsync(userId, ct);

            // bu gün tamamlanan quest sayı
            var completedTodayCount = await _db.QuestAttempts.AsNoTracking()
                .CountAsync(a => a.UserId == userId && a.CompletedDateUtc == today, ct);

            var remaining = Math.Max(0, dailyLimit - completedTodayCount);

            // bu gün tamamlanan quest-lərin listi (Completed flag üçün)
            var completedQuestIds = await _db.QuestAttempts.AsNoTracking()
                .Where(a => a.UserId == userId && a.CompletedDateUtc == today)
                .Select(a => a.QuestId)
                .ToListAsync(ct);

            // eligible quest-lər (Free user üçün Advanced çıxarılır)
            var eligible = await _db.Quests.AsNoTracking()
                .Where(q => q.IsActive)
                .Where(q => advancedAllowed || q.Difficulty != QuestDifficulty.Advanced)
                .Where(q => !q.IsPremiumOnly || advancedAllowed) // PremiumOnly yalnız premium-a
                .ToListAsync(ct);

            // “Today rotation” — eyni gün üçün deterministic order
            var dayKey = today.ToString("yyyyMMdd");
            var ordered = eligible
                .OrderBy(q => DeterministicScore(q.Id, dayKey))
                .Take(take)
                .Select(q => new TodayQuestDto(
                    q.Id,
                    q.Title,
                    q.SubjectCode.ToString(),
                    q.Difficulty.ToString(),
                    q.BaseXp,
                    completedQuestIds.Contains(q.Id)
                ))
                .ToList();

            return new TodayQuestsResponse(dailyLimit, completedTodayCount, remaining, ordered);
        }

        private static long DeterministicScore(Guid id, string dayKey)
        {
            // GUID + dayKey → stabil “random-like” sıralama
            var bytes = Encoding.UTF8.GetBytes(id.ToString("N") + ":" + dayKey);
            var hash = SHA256.HashData(bytes);
            return BitConverter.ToInt64(hash, 0);
        }
    }
}
