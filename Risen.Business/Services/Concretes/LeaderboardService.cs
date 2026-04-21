using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Leaderboards;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _db;
        public LeaderboardService(AppDbContext db) => _db = db;

        public async Task<LeaderboardResponse> GetGlobalAsync(LeagueCode? league, int limit, int offset, CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 100);
            offset = Math.Max(0, offset);

            var q = BuildBaseQuery();

            if (league.HasValue)
                q = q.Where(x => x.LeagueCode == league.Value);

            var total = await q.CountAsync(ct);

            // CS0853 fix: əvvəl SQL -> list, sonra rank ver (memory)
            var page = await q
                .OrderByDescending(x => x.TotalXp)
                .ThenBy(x => x.UserId)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var items = page.Select((x, i) => new LeaderboardEntryDto(
                Rank: offset + i + 1,
                UserId: x.UserId,
                DisplayName: x.DisplayName,
                TotalXp: x.TotalXp,
                League: x.LeagueCode.ToString(),
                UniversityName: x.UniversityName
            )).ToList();

            return new LeaderboardResponse(limit, offset, items, total);
        }

        public async Task<LeaderboardResponse> GetUniversityAsync(Guid universityId, LeagueCode? league, int limit, int offset, CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 100);
            offset = Math.Max(0, offset);

            var q = BuildBaseQuery()
                .Where(x => x.UniversityId == universityId);

            if (league.HasValue)
                q = q.Where(x => x.LeagueCode == league.Value);

            var total = await q.CountAsync(ct);

            var page = await q
                .OrderByDescending(x => x.TotalXp)
                .ThenBy(x => x.UserId)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var items = page.Select((x, i) => new LeaderboardEntryDto(
                Rank: offset + i + 1,
                UserId: x.UserId,
                DisplayName: x.DisplayName,
                TotalXp: x.TotalXp,
                League: x.LeagueCode.ToString(),
                UniversityName: x.UniversityName
            )).ToList();

            return new LeaderboardResponse(limit, offset, items, total);
        }

        private IQueryable<Row> BuildBaseQuery()
        {
            // UserStats + Users + LeagueTiers + Universities (left join)
            var q =
                from s in _db.UserStats.AsNoTracking()
                join u in _db.Users.AsNoTracking() on s.UserId equals u.Id
                join t in _db.LeagueTiers.AsNoTracking() on s.CurrentLeagueTierId equals t.Id
                join uni in _db.Universities.AsNoTracking() on u.UniversityId equals uni.Id into uniLeft
                from uni in uniLeft.DefaultIfEmpty()
                select new Row
                {
                    UserId = s.UserId,
                    DisplayName = u.FullName,
                    TotalXp = s.TotalXp,
                    LeagueCode = t.Code,
                    UniversityId = u.UniversityId,
                    UniversityName = uni != null ? uni.Name : null
                };

            return q;
        }

        private sealed class Row
        {
            public Guid UserId { get; set; }
            public string DisplayName { get; set; } = default!;
            public long TotalXp { get; set; }
            public LeagueCode LeagueCode { get; set; }
            public Guid? UniversityId { get; set; }
            public string? UniversityName { get; set; }
        }

        public async Task<int> GetUserRankAsync(Guid userId, Guid? universityId, CancellationToken ct)
        {
            // get user's total xp
            var userRow = await (
                from s in _db.UserStats.AsNoTracking()
                join u in _db.Users.AsNoTracking() on s.UserId equals u.Id
                where s.UserId == userId
                select new { s.TotalXp, u.UniversityId }
            ).FirstOrDefaultAsync(ct);

            if (userRow is null) return -1;

            var targetXp = userRow.TotalXp;

            var q = from s in _db.UserStats.AsNoTracking()
                    where s.TotalXp > targetXp
                    select s.UserId;

            if (universityId.HasValue)
            {
                q = from s in _db.UserStats.AsNoTracking()
                    join u in _db.Users.AsNoTracking() on s.UserId equals u.Id
                    where s.TotalXp > targetXp && u.UniversityId == universityId.Value
                    select s.UserId;
            }

            var higherCount = await q.CountAsync(ct);

            // rank is count of users with more XP + 1
            return higherCount + 1;
        }
    }
}
