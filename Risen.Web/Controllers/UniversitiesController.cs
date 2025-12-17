using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Risen.Business.Integrations.Hipolabs;
using Risen.DataAccess.Data;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHipolabsClient _hipo;
        private readonly IMemoryCache _cache;

        public UniversitiesController(AppDbContext db, IHipolabsClient hipo, IMemoryCache cache)
        {
            _db = db;
            _hipo = hipo;
            _cache = cache;
        }

        // GET /api/universities/suggest?q=aze&limit=10
        [HttpGet("suggest")]
        public async Task<ActionResult<string[]>> Suggest(
            [FromQuery] string q,
            [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Ok(Array.Empty<string>());

            q = q.Trim();
            limit = Math.Clamp(limit, 1, 20);

            var cacheKey = $"uni:suggest:{q}:{limit}";
            if (_cache.TryGetValue(cacheKey, out string[] cached))
                return Ok(cached);

            // 1) DB: StartsWith üstün, sonra Contains
            var startsDb = await _db.Universities
                .Where(u => u.Name.StartsWith(q))
                .OrderBy(u => u.Name)
                .Select(u => u.Name)
                .Take(limit)
                .ToListAsync(ct);

            var remaining = limit - startsDb.Count;

            var containsDb = remaining > 0
                ? await _db.Universities
                    .Where(u => !startsDb.Contains(u.Name) && EF.Functions.Like(u.Name, $"%{q}%"))
                    .OrderBy(u => u.Name)
                    .Select(u => u.Name)
                    .Take(remaining)
                    .ToListAsync(ct)
                : new List<string>();

            // 2) Hipolabs: bir az çox götürüb filter edirik
            var hipo = await _hipo.SearchAsync(q, country: null, limit: 50, offset: 0, ct);

            var hipoNames = hipo
                .Select(x => x.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            // startswith hipolabs
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
            return Ok(result);
        }

        // GET /api/universities/search?q=azerbaijan&country=Azerbaijan&limit=20&offset=0
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            [FromQuery] string? country = null,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Ok(new { q, country, limit, offset, local = Array.Empty<object>(), items = Array.Empty<object>() });

            q = q.Trim();
            limit = Math.Clamp(limit, 1, 50);
            offset = Math.Max(0, offset);

            // DB-dən yerli nəticələr (top 10)
            var local = await _db.Universities
                .Where(u => EF.Functions.Like(u.Name, $"%{q}%"))
                .OrderBy(u => u.Name)
                .Select(u => new { name = u.Name })
                .Take(10)
                .ToListAsync(ct);

            // Hipolabs-dan paginated nəticələr
            var hipo = await _hipo.SearchAsync(q, country, limit, offset, ct);

            var items = hipo.Select(x => new
            {
                name = x.Name,
                country = x.Country,
                stateProvince = x.StateProvince,
                domain = x.Domains?.FirstOrDefault() ?? x.Domain,
                webPage = x.WebPages?.FirstOrDefault() ?? x.WebPage
            });

            return Ok(new
            {
                q,
                country,
                limit,
                offset,
                local,
                items
            });
        }

    }
}
