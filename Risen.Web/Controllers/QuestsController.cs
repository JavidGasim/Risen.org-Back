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

        public QuestsController(IQuestService quest) => _quest = quest;

        [Authorize]
        [HttpPost("submit")]
        public async Task<ActionResult<SubmitQuestAnswerResponse>> Submit([FromBody] SubmitQuestAnswerRequest req, CancellationToken ct)
        {
            var idStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr))
                return Unauthorized("User id claim is missing.");

            var userId = Guid.Parse(idStr);

            return Ok(await _quest.SubmitAsync(userId, req, ct));
        }
    }
}
