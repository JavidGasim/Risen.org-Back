using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.DataAccess.Data;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaguesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<LeaguesController> _logger;
        public LeaguesController(AppDbContext db, ILogger<LeaguesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("User {UserId} accessed /api/leagues/me", userId);
            var stats = await _db.UserStats.AsNoTracking()
                .Include(s => s.CurrentLeagueTier)
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);

            if (stats is null)
            {
                _logger.LogInformation("User {UserId} has no stats, returning default values.", userId);
                return Ok(new { totalXp = 0, league = "Rookie" });
            }

            _logger.LogInformation("User {UserId} has stats, returning values.", userId);
            return Ok(new
            {
                totalXp = stats.TotalXp,
                league = stats.CurrentLeagueTier.Code.ToString(),
                leagueName = stats.CurrentLeagueTier.Name
            });
        }
    }
}
