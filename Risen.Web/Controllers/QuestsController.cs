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
        private readonly IQuestService _service;

        public QuestsController(IQuestService service)
        {
            _service = service;
        }

        [HttpGet("{questId:guid}")]
        public async Task<ActionResult<QuestDto>> GetQuest(Guid questId, CancellationToken ct)
        {
            var dto = await _service.GetQuestAsync(questId, ct);
            return Ok(dto);
        }

        [Authorize]
        [HttpPost("{questId:guid}/submit")]
        public async Task<ActionResult<SubmitQuestAnswerResponse>> Submit(
            Guid questId,
            [FromBody] SubmitQuestAnswerRequest req,
            CancellationToken ct)
        {
            // JWT claim-lərinizə uyğunlaşdırın:
            var userId = Guid.Parse(User.FindFirst("sub")!.Value);

            var res = await _service.SubmitAnswerAsync(questId, userId, req.SelectedOptionIndex, ct);
            return Ok(res);
        }
    }
}
