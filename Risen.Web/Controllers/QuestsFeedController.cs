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
    public class QuestsFeedController : ControllerBase
    {
        private readonly IQuestFeedService _feed;

        public QuestsFeedController(IQuestFeedService feed)
        {
            _feed = feed;
        }

        // GET /api/quests/today?take=20
        [Authorize]
        [HttpGet("today")]
        public async Task<ActionResult<GetTodayQuestsResponse>> Today([FromQuery] int take = 20, CancellationToken ct = default)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _feed.GetTodayAsync(userId, take, ct));
        }
    }
}
