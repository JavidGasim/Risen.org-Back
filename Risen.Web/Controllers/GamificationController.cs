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
        public GamificationController(IXpService xp) => _xp = xp;

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
                return Unauthorized("User id claim is missing.");

            var userId = Guid.Parse(idStr);

            var res = await _xp.AwardAsync(userId, req, ct);
            return Ok(res);
        }

    }
}
