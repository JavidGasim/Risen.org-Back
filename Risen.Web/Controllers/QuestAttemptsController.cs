using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Quests;
using Risen.DataAccess.Data;

namespace Risen.Web.Controllers
{
    [Route("api/quest-attempts")]
    [ApiController]
    public class QuestAttemptsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public QuestAttemptsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestAttemptDto>>> List([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var q = await _db.QuestAttempts
                .Include(a => a.User)
                .Include(a => a.Quest)
                .OrderByDescending(a => a.CompletedAtUtc)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(ct);

            var dto = q.Select(a => new QuestAttemptDto(
                a.Id,
                a.UserId,
                a.User?.Email,
                a.QuestId,
                a.Quest?.QuestionText,
                a.SelectedOptionId,
                a.IsCorrect,
                a.AwardedXp,
                a.CompletedAtUtc,
                a.CompletedDateUtc
            ));

            return Ok(dto);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuestAttemptDto>> GetById(Guid id, CancellationToken ct)
        {
            var a = await _db.QuestAttempts
                .Include(x => x.User)
                .Include(x => x.Quest)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (a is null) return NotFound();

            var dto = new QuestAttemptDto(
                a.Id,
                a.UserId,
                a.User?.Email,
                a.QuestId,
                a.Quest?.QuestionText,
                a.SelectedOptionId,
                a.IsCorrect,
                a.AwardedXp,
                a.CompletedAtUtc,
                a.CompletedDateUtc
            );

            return Ok(dto);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var existing = await _db.QuestAttempts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (existing is null) return NotFound();

            _db.QuestAttempts.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
