using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Leaderboards;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardsController : ControllerBase
    {
        private readonly ILeaderboardService _svc;
        private readonly AppDbContext _db;

        public LeaderboardsController(ILeaderboardService svc, AppDbContext db)
        {
            _svc = svc;
            _db = db;
        }

        // GET /api/leaderboards/global?league=Gold&limit=50&offset=0
        [HttpGet("global")]
        public async Task<ActionResult<LeaderboardResponse>> Global(
            [FromQuery] string? league,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0,
            CancellationToken ct = default)
        {
            var leagueCode = ParseLeague(league);
            return Ok(await _svc.GetGlobalAsync(leagueCode, limit, offset, ct));
        }

        // GET /api/leaderboards/university/{universityId}?league=Gold&limit=50&offset=0
        [HttpGet("university/{universityId:guid}")]
        public async Task<ActionResult<LeaderboardResponse>> University(
            Guid universityId,
            [FromQuery] string? league,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0,
            CancellationToken ct = default)
        {
            var leagueCode = ParseLeague(league);
            return Ok(await _svc.GetUniversityAsync(universityId, leagueCode, limit, offset, ct));
        }

        // GET /api/leaderboards/my-university
        [Authorize]
        [HttpGet("my-university")]
        public async Task<ActionResult<LeaderboardResponse>> MyUniversity(
            [FromQuery] string? league,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0,
            CancellationToken ct = default)
        {
            var idStr =
    User.FindFirstValue(ClaimTypes.NameIdentifier) ??
    User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
    User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr))
                return Unauthorized("User id claim is missing.");

            var userId = Guid.Parse(idStr);

            var uniId = await _db.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UniversityId)
                .FirstOrDefaultAsync(ct);


            if (uniId is null)
                return Ok(new LeaderboardResponse(limit, offset, Array.Empty<LeaderboardEntryDto>()));

            var leagueCode = ParseLeague(league);
            return Ok(await _svc.GetUniversityAsync(uniId.Value, leagueCode, limit, offset, ct));
        }

        private static LeagueCode? ParseLeague(string? league)
        {
            if (string.IsNullOrWhiteSpace(league)) return null;
            return Enum.TryParse<LeagueCode>(league.Trim(), ignoreCase: true, out var code) ? code : null;
        }
    }
}
