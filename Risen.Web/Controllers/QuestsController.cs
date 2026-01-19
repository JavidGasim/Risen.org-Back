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
        private readonly IQuestService _questService;

        public QuestsController(IQuestService questService)
        {
            _questService = questService;
        }

        [Authorize]
        [HttpPost("{questId:guid}/submit")]
        public async Task<ActionResult<SubmitQuestAnswerResponse>> Submit(
    Guid questId,
    [FromBody] SubmitQuestAnswerRequest req,
    CancellationToken ct)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return BadRequest(new { error = "Invalid or missing user id claim." });

                var res = await _questService.SubmitAsync(userId, questId, req.SelectedOptionIndex, ct);
                return Ok(res);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = default;
            var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out userId);
        }
    }
}
