using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Risen.Business.Integrations.Hipolabs;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Universities;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class UniversitySuggestService : IUniversitySuggestService
    {
        private readonly AppDbContext _db;
        private readonly IHipolabsClient _hipo;
        private readonly IMemoryCache _cache;

        public UniversitySuggestService(AppDbContext db, IHipolabsClient hipo, IMemoryCache cache)
        {
            _db = db;
            _hipo = hipo;
            _cache = cache;
        }

        public async Task<string[]> SuggestAsync(string q, int limit, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Array.Empty<string>();

            q = q.Trim();
            limit = Math.Clamp(limit, 1, 20);

            // cache key: case-insensitive olsun
            var qKey = q.ToUpperInvariant();
            var cacheKey = $"uni:suggest:{qKey}:{limit}";

            if (_cache.TryGetValue(cacheKey, out string[] cached))
                return cached;

            // 1) DB: StartsWith üstün, sonra Contains
            var startsDb = await _db.Universities.AsNoTracking()
                .Where(u => u.Name.StartsWith(q))
                .OrderBy(u => u.Name)
                .Select(u => u.Name)
                .Take(limit)
                .ToListAsync(ct);

            var remaining = limit - startsDb.Count;

            var containsDb = remaining > 0
                ? await _db.Universities.AsNoTracking()
                    .Where(u => !startsDb.Contains(u.Name) && EF.Functions.Like(u.Name, $"%{q}%"))
                    .OrderBy(u => u.Name)
                    .Select(u => u.Name)
                    .Take(remaining)
                    .ToListAsync(ct)
                : new List<string>();

            // 2) Hipolabs
            var hipo = await _hipo.SearchAsync(q, country: null, limit: 50, offset: 0, ct);

            var hipoNames = hipo
                .Select(x => x.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            var startsHipo = hipoNames
                .Where(n => n.StartsWith(q, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n)
                .ToList();

            var containsHipo = hipoNames
                .Where(n => !startsHipo.Contains(n, StringComparer.OrdinalIgnoreCase) &&
                            n.Contains(q, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n)
                .ToList();

            var result = startsDb
                .Concat(containsDb)
                .Concat(startsHipo)
                .Concat(containsHipo)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(limit)
                .ToArray();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        public async Task<UniversitySearchResponseDto> SearchAsync(
            string q, string? country, int limit, int offset, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return new UniversitySearchResponseDto(
                    Q: q ?? "",
                    Country: country,
                    Limit: Math.Clamp(limit, 1, 50),
                    Offset: Math.Max(0, offset),
                    Local: Array.Empty<UniversityLocalDto>(),
                    Items: Array.Empty<UniversitySearchItemDto>()
                );
            }

            q = q.Trim();
            limit = Math.Clamp(limit, 1, 50);
            offset = Math.Max(0, offset);

            var local = await _db.Universities.AsNoTracking()
                .Where(u => EF.Functions.Like(u.Name, $"%{q}%"))
                .OrderBy(u => u.Name)
                .Select(u => new UniversityLocalDto(u.Name))
                .Take(10)
                .ToListAsync(ct);

            var hipo = await _hipo.SearchAsync(q, country, limit, offset, ct);

            // sənin Hipolabs modelinə uyğun map (controller-dəki kimi)
            var items = hipo.Select(x => new UniversitySearchItemDto(
         Name: x.Name,
         Country: x.Country,
         StateProvince: x.StateProvince,
         Domain: x.Domains?.FirstOrDefault(),
         WebPage: x.WebPages?.FirstOrDefault()
     )).ToList();


            return new UniversitySearchResponseDto(
                Q: q,
                Country: country,
                Limit: limit,
                Offset: offset,
                Local: local,
                Items: items
            );
        }


    }
}