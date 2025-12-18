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

        public LeaderboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<LeaderboardResponse> GetGlobalAsync(LeagueCode? league, int limit, int offset, CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 100);
            offset = Math.Max(0, offset);

            var query = BuildBaseQuery();

            if (league is not null)
                query = query.Where(x => x.LeagueCode == league.Value);

            // DB-dən yalnız “page” götürürük
            var page = await query
                .OrderByDescending(x => x.TotalXp)
                .ThenBy(x => x.UserId)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            // Rank-i memory-də veririk (EF translation problemi olmur)
            var items = page
                .Select((x, idx) => new LeaderboardEntryDto(
                    offset + idx + 1,
                    x.UserId,
                    x.DisplayName,
                    x.TotalXp,
                    x.LeagueCode.ToString(),
                    x.UniversityName
                ))
                .ToList();

            return new LeaderboardResponse(limit, offset, items);
        }

        public async Task<LeaderboardResponse> GetUniversityAsync(Guid universityId, LeagueCode? league, int limit, int offset, CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 100);
            offset = Math.Max(0, offset);

            var query = BuildBaseQuery()
                .Where(x => x.UniversityId == universityId);

            if (league is not null)
                query = query.Where(x => x.LeagueCode == league.Value);

            var page = await query
                .OrderByDescending(x => x.TotalXp)
                .ThenBy(x => x.UserId)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var items = page
                .Select((x, idx) => new LeaderboardEntryDto(
                    offset + idx + 1,
                    x.UserId,
                    x.DisplayName,
                    x.TotalXp,
                    x.LeagueCode.ToString(),
                    x.UniversityName
                ))
                .ToList();

            return new LeaderboardResponse(limit, offset, items);
        }

        /// <summary>
        /// Leaderboard üçün minimal select (Include yoxdur). EF Core üçün sürətli query-dir.
        /// </summary>
        private IQueryable<BaseRow> BuildBaseQuery()
        {
            // UserStats -> Users -> LeagueTiers (+ left join Universities)
            return
                from s in _db.UserStats.AsNoTracking()
                join u in _db.Users.AsNoTracking() on s.UserId equals u.Id
                join t in _db.LeagueTiers.AsNoTracking() on s.CurrentLeagueTierId equals t.Id
                join uni in _db.Universities.AsNoTracking() on u.UniversityId equals uni.Id into uniLeft
                from uni in uniLeft.DefaultIfEmpty()
                select new BaseRow
                {
                    UserId = u.Id,

                    // Null riskini minimum etmək üçün:
                    DisplayName = (((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim()),

                    TotalXp = s.TotalXp,
                    LeagueCode = t.Code,
                    UniversityId = u.UniversityId,
                    UniversityName = uni != null ? uni.Name : null
                };
        }

        private sealed class BaseRow
        {
            public Guid UserId { get; set; }
            public string DisplayName { get; set; } = default!;
            public long TotalXp { get; set; }
            public LeagueCode LeagueCode { get; set; }
            public Guid? UniversityId { get; set; }
            public string? UniversityName { get; set; }
        }
    }
}
