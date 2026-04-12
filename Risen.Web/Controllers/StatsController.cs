using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Stats;
using Risen.DataAccess.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _stats;
        private readonly ILogger<StatsController> _logger;

        public StatsController(IStatsService stats, ILogger<StatsController> logger)
        {
            _stats = stats;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<MeStatsDto>> Me(CancellationToken ct = default)
        {
            var idStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr))
            {
                _logger.LogWarning("User id claim is missing in the token.");
                return Unauthorized("User id claim is missing.");
            }

            var userId = Guid.Parse(idStr);

            var dto = await _stats.GetMeAsync(userId, ct);
            _logger.LogInformation("Retrieved stats for user {UserId}: {@Stats}", userId, dto);
            return Ok(dto);
        }

    }
}
