using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Stats;
using Risen.DataAccess.Data;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StatsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/stats/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<MyStatsResponse>> Me(CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var data = await (
                from u in _db.Users.AsNoTracking()
                where u.Id == userId
                join s in _db.UserStats.AsNoTracking() on u.Id equals s.UserId into sLeft
                from s in sLeft.DefaultIfEmpty()
                join t in _db.LeagueTiers.AsNoTracking() on s.CurrentLeagueTierId equals t.Id into tLeft
                from t in tLeft.DefaultIfEmpty()
                join uni in _db.Universities.AsNoTracking() on u.UniversityId equals uni.Id into uniLeft
                from uni in uniLeft.DefaultIfEmpty()
                select new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.FullName,
                    u.Email,
                    u.LastOnlineAtUtc,
                    UniversityName = uni != null ? uni.Name : null,

                    TotalXp = s != null ? s.TotalXp : 0,
                    League = t != null ? t.Code.ToString() : "Rookie",

                    CurrentStreak = s != null ? s.CurrentStreak : 0,
                    LongestStreak = s != null ? s.LongestStreak : 0,
                    LastStreakDateUtc = s != null ? s.LastStreakDateUtc : null
                }
            ).FirstAsync(ct);

            return Ok(new MyStatsResponse(
                data.Id,
                data.FirstName,
                data.LastName,
                data.FullName,
                data.Email ?? "",
                data.UniversityName,
                data.LastOnlineAtUtc,
                data.TotalXp,
                data.League,
                data.CurrentStreak,
                data.LongestStreak,
                data.LastStreakDateUtc
            ));
        }
    }
}
