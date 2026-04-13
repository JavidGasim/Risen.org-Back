using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Universities;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class UniversityCandidateService : IUniversityCandidateService
    {
        private readonly AppDbContext _db;

        public UniversityCandidateService(AppDbContext db) => _db = db;

        public async Task<CandidatesResponse> GetCandidatesAsync(
            LeagueCode? minLeague,
            decimal? minScore,
            string? country,
            int limit,
            int offset,
            CancellationToken ct)
        {
            limit = Math.Clamp(limit, 1, 100);
            offset = Math.Max(0, offset);

            var q =
                from u in _db.Users.AsNoTracking()
                join s in _db.UserStats.AsNoTracking() on u.Id equals s.UserId
                join t in _db.LeagueTiers.AsNoTracking() on s.CurrentLeagueTierId equals t.Id
                join uni in _db.Universities.AsNoTracking() on u.UniversityId equals uni.Id into uniLeft
                from uni in uniLeft.DefaultIfEmpty()
                select new
                {
                    u.Id,
                    u.FullName,
                    UniversityName = uni != null ? uni.Name : null,
                    Country = uni != null ? uni.Country : null,
                    t.Code,
                    t.Name,
                    t.SortOrder,
                    s.TotalXp,
                    s.RisenScore,
                    s.CurrentStreak,
                    s.LongestStreak
                };

            // Minimum league filteri
            if (minLeague.HasValue)
            {
                var minSortOrder = await _db.LeagueTiers.AsNoTracking()
                    .Where(t => t.Code == minLeague.Value)
                    .Select(t => t.SortOrder)
                    .FirstOrDefaultAsync(ct);

                q = q.Where(x => x.SortOrder >= minSortOrder);
            }

            // Minimum Risen Score filteri
            if (minScore.HasValue)
                q = q.Where(x => x.RisenScore >= minScore.Value);

            // Ölkə filteri
            if (!string.IsNullOrWhiteSpace(country))
                q = q.Where(x => x.Country == country);

            var total = await q.CountAsync(ct);

            var page = await q
                .OrderByDescending(x => x.RisenScore)
                .ThenByDescending(x => x.TotalXp)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var items = page.Select(x => new CandidateDto(
                UserId: x.Id,
                FullName: x.FullName,
                UniversityName: x.UniversityName,
                Country: x.Country,
                LeagueCode: x.Code.ToString(),
                LeagueName: x.Name,
                TotalXp: x.TotalXp,
                RisenScore: x.RisenScore,
                CurrentStreak: x.CurrentStreak,
                LongestStreak: x.LongestStreak
            )).ToList();

            return new CandidatesResponse(limit, offset, total, items);
        }
    }
}
