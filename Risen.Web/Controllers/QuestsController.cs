using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Quests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly IQuestService _svc;

        public QuestsController(IQuestService svc)
        {
            _svc = svc;
        }

        // POST /api/quests/complete
        [Authorize]
        [HttpPost("complete")]
        [ProducesResponseType(typeof(CompleteQuestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CompleteQuestResponse>> Complete(
            [FromBody] CompleteQuestRequest req,
            CancellationToken ct = default)
        {
            if (req is null || req.QuestId == Guid.Empty)
                return BadRequest("QuestId is required.");

            var idStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr))
                return Unauthorized("User id claim is missing.");

            if (!Guid.TryParse(idStr, out var userId))
                return Unauthorized("User id claim is invalid.");

            try
            {
                var res = await _svc.CompleteAsync(userId, req, ct);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                // Service səviyyəsində verdiyin biznes xətaları burada 400 kimi qaytarırıq.
                return BadRequest(ex.Message);
            }
        }
    }
}
