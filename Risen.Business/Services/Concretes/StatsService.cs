using Microsoft.EntityFrameworkCore;
using Risen.Business.Exceptions;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Stats;
using Risen.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class StatsService : IStatsService
    {
        private readonly AppDbContext _db;
        private readonly IEntitlementService _entitlement;

        public StatsService(AppDbContext db, IEntitlementService entitlement)
        {
            _db = db;
            _entitlement = entitlement;
        }

        public async Task<Risen.Entities.Entities.UserStats> EnsureStatsAsync(Guid userId, CancellationToken ct)
        {
            var stats = await _db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (stats is not null) return stats;

            var rookieId = await _db.LeagueTiers
                .Where(t => t.Code == Risen.Entities.Entities.LeagueCode.Rookie)
                .Select(t => t.Id)
                .FirstAsync(ct);

            stats = new Risen.Entities.Entities.UserStats
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
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // concurrent creation — re-query
                var existing = await _db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId, ct);
                if (existing is not null) return existing;
                throw;
            }

            return stats;
        }

        public async Task<MeStatsDto> GetMeAsync(Guid userId, CancellationToken ct)
        {
            var row = await (
                from u in _db.Users.AsNoTracking()
                join s in _db.UserStats.AsNoTracking() on u.Id equals s.UserId
                join t in _db.LeagueTiers.AsNoTracking() on s.CurrentLeagueTierId equals t.Id
                join uni in _db.Universities.AsNoTracking() on u.UniversityId equals uni.Id into uniLeft
                from uni in uniLeft.DefaultIfEmpty()
                where u.Id == userId
                select new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.FullName,
                    u.CreatedAtUtc,
                    u.LastOnlineAtUtc,
                    UniversityName = uni != null ? uni.Name : null,

                    s.TotalXp,
                    s.CurrentStreak,
                    s.LongestStreak,
                    s.LastStreakDateUtc,
                    s.RisenScore,
                    LeagueCode = t.Code.ToString(),
                    LeagueName = t.Name
                }
            ).FirstOrDefaultAsync(ct);

            if (row is null)
                throw new NotFoundException("User stats not found...");

            var (isPremium, plan) = await _entitlement.GetUserEntitlementAsync(userId, ct);

            return new MeStatsDto(
       UserId: row.Id,
       FirstName: row.FirstName,
       LastName: row.LastName,
       FullName: row.FullName,
       Email: row.Email ?? "",
       UniversityName: row.UniversityName,

       TotalXp: row.TotalXp,
       LeagueCode: row.LeagueCode,
       LeagueName: row.LeagueName,
       RisenScore: row.RisenScore,  // ← əlavə et

       CurrentStreak: row.CurrentStreak,
       LongestStreak: row.LongestStreak,
       LastStreakDateUtc: row.LastStreakDateUtc,

       CreatedAtUtc: row.CreatedAtUtc,
       LastOnlineAtUtc: row.LastOnlineAtUtc,

       Plan: plan,
       IsPremium: isPremium
   );

        }
    }
}
