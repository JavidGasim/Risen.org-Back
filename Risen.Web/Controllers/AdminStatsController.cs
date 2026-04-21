using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Utils;
using Risen.DataAccess.Data;

namespace Risen.Web.Controllers
{
    [Route("api/admin/stats")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminStatsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminStatsController> _logger;

        public AdminStatsController(AppDbContext db, ILogger<AdminStatsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpPost("recalculate/user/{userId:guid}")]
        public async Task<IActionResult> RecalculateUser(Guid userId, CancellationToken ct)
        {
            var stats = await _db.UserStats.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (stats is null) return NotFound();

            var tier = await _db.LeagueTiers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == stats.CurrentLeagueTierId, ct);
            var weight = tier?.Weight ?? 0;
            stats.RisenScore = RisenScoreCalculator.Calculate(weight, stats.TotalXp, stats.CurrentStreak);
            await _db.SaveChangesAsync(ct);
            return Ok(new { userId = stats.UserId, risenScore = stats.RisenScore });
        }

        [HttpPost("recalculate/all")]
        public async Task<IActionResult> RecalculateAll(CancellationToken ct)
        {
            var tiers = await _db.LeagueTiers.AsNoTracking().ToListAsync(ct);
            var statsList = await _db.UserStats.ToListAsync(ct);

            foreach (var s in statsList)
            {
                var t = tiers.FirstOrDefault(x => x.Id == s.CurrentLeagueTierId);
                var weight = t?.Weight ?? 0;
                s.RisenScore = RisenScoreCalculator.Calculate(weight, s.TotalXp, s.CurrentStreak);
            }

            await _db.SaveChangesAsync(ct);
            return Ok(new { count = statsList.Count });
        }
    }
}
