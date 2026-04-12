using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Gamification;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XpController : ControllerBase
    {
        private readonly IXpService _xp;
        private readonly ILogger _logger;

        public XpController(IXpService xp, ILogger<XpController> logger)
        {
            _xp = xp;
            _logger = logger;
        }

        // POST /api/xp/award
        [Authorize(Roles = "Admin")]
        [HttpPost("award")]
        public async Task<ActionResult<AwardXpResponse>> Award([FromBody] AwardXpRequest req, CancellationToken ct = default)
        {
            var idStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr))
                return Unauthorized("User id claim is missing.");

            var userId = Guid.Parse(idStr);

            var res = await _xp.AwardAsync(userId, req, ct);
            _logger.LogInformation("Awarded {FinalXp} XP to user {UserId} (new total: {NewTotalXp}, new league: {NewLeague}) for source {SourceType}:{SourceKey}",
                res.FinalXp, userId, res.NewTotalXp, res.NewLeague, req.SourceType, req.SourceKey);
            return Ok(res);
        }

    }
}
