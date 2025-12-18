using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Gamification;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamificationController : ControllerBase
    {
        private readonly IXpService _xp;
        public GamificationController(IXpService xp) => _xp = xp;

        [Authorize]
        [HttpPost("claim-xp")]
        public async Task<ActionResult<ClaimXpResponse>> ClaimXp([FromBody] ClaimXpRequest req, CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _xp.ClaimAsync(userId, req, ct));
        }
    }
}
