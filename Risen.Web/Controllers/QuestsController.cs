using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Quests;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly IQuestService _quests;
        public QuestsController(IQuestService quests) => _quests = quests;

        [Authorize]
        [HttpPost("complete")]
        public async Task<ActionResult<CompleteQuestResponse>> Complete([FromBody] CompleteQuestRequest req, CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _quests.CompleteAsync(userId, req, ct));
        }
    }
}
