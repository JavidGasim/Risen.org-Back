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
    public class GamificationController : ControllerBase
    {
        private readonly IXpService _xp;
        private readonly ILogger<GamificationController> _logger;
        public GamificationController(IXpService xp, ILogger<GamificationController> logger)
        {
            _xp = xp;
            _logger = logger;
        }

        // POST /api/gamification/award-xp
        // Diqqət: bunu açıq saxlamayın. Admin-lə məhdudlaşdırın.
        [Authorize(Roles = "Admin")]
        [HttpPost("award-xp")]
        public async Task<ActionResult<AwardXpResponse>> AwardXp([FromBody] AwardXpRequest req, CancellationToken ct = default)
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

            var adminId = Guid.Parse(idStr);
            var targetId = req.TargetUserId ?? adminId;

            var res = await _xp.AwardAsync(adminId, req, ct);

            _logger.LogInformation("Admin {AdminId} awarded {FinalXp} XP to user {TargetId}. New total XP: {NewTotalXp}, New league: {NewLeague}.",
                adminId, res.FinalXp, targetId, res.NewTotalXp, res.NewLeague);
            return Ok(res);
        }

    }
}
