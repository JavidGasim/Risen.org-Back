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
        private readonly IQuestFeedService _feed;
        private readonly IQuestQueryService _query;
        private readonly IQuestService _quest;

        public QuestsController(IQuestFeedService feed, IQuestQueryService query, IQuestService quest)
        {
            _feed = feed;
            _query = query;
            _quest = quest;
        }

        // GET /api/quests/today?take=10
        [Authorize]
        [HttpGet("today")]
        public async Task<ActionResult<TodayQuestsResponse>> Today([FromQuery] int take = 10, CancellationToken ct = default)
        {
            var userId = GetUserIdOrThrow();
            return Ok(await _feed.GetTodayAsync(userId, take, ct));
        }

        // GET /api/quests/catalog
        [Authorize]
        [HttpGet("catalog")]
        public async Task<ActionResult<IReadOnlyList<QuestListItemDto>>> Catalog(CancellationToken ct = default)
        {
            var userId = GetUserIdOrThrow();
            return Ok(await _query.GetCatalogAsync(userId, ct));
        }

        // GET /api/quests/{id}
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuestListItemDto>> GetById(Guid id, CancellationToken ct = default)
        {
            var userId = GetUserIdOrThrow();
            var item = await _query.GetByIdAsync(userId, id, ct);
            if (item is null) return NotFound();
            return Ok(item);
        }

        // POST /api/quests/complete
        [Authorize]
        [HttpPost("complete")]
        public async Task<ActionResult<CompleteQuestResponse>> Complete([FromBody] CompleteQuestRequest req, CancellationToken ct = default)
        {
            var userId = GetUserIdOrThrow();
            return Ok(await _quest.CompleteAsync(userId, req, ct));
        }

        private Guid GetUserIdOrThrow()
        {
            var idStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var userId))
                throw new InvalidOperationException("User id claim is missing or invalid.");

            return userId;
        }

    }
}
