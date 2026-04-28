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
        private readonly ILogger<QuestsFeedController> _logger;

        public QuestsFeedController(IQuestFeedService feed, ILogger<QuestsFeedController> logger)
        {
            _feed = feed;
            _logger = logger;
        }

        // GET /api/quests/today?take=20
        [Authorize]
        [HttpGet("today")]
        public async Task<ActionResult<TodayQuestsResponse>> Today([FromQuery] int take = 20, CancellationToken ct = default)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("User {UserId} requested today's quests feed with take={Take}", userId, take);
            return Ok(await _feed.GetTodayAsync(userId, take, ct));
        }

        // GET /api/quests/all?limit=50&offset=0&includeInactive=false
        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<IReadOnlyList<QuestListItemDto>>> All([FromQuery] int limit = 50, [FromQuery] int offset = 0, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        {
            _logger.LogInformation("Quests feed: all requested by {User}", User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var items = await _feed.GetAllAsync(userId, limit, offset, includeInactive, ct);
            return Ok(items);
        }
    }
}
