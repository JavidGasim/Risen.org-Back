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
        private readonly IQuestService _quest;
        private readonly ILogger<QuestsController> _logger;

        public QuestsController(IQuestService quest, ILogger<QuestsController> logger)
        {
            _quest = quest;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("submit")]
        public async Task<ActionResult<SubmitQuestAnswerResponse>> Submit([FromBody] SubmitQuestAnswerRequest req, CancellationToken ct)
        {
            var idStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr))
            {
                _logger.LogWarning("User id claim is missing in the token.");
                return Unauthorized("User id claim is missing.");
            }

            var userId = Guid.Parse(idStr);

            _logger.LogInformation("User {UserId} is submitting an answer for quest {QuestId}.", userId, req.QuestId);

            return Ok(await _quest.SubmitAsync(userId, req, ct));
        }
    }
}
