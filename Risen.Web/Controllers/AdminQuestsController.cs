using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Administration;
using Risen.Contracts.Quests;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/admin/quests")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminQuestsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminQuestsController> _logger;
        private readonly Risen.Business.Services.Abstracts.IAdminAuditService _audit;

        public AdminQuestsController(AppDbContext db, ILogger<AdminQuestsController> logger, Risen.Business.Services.Abstracts.IAdminAuditService audit)
        {
            _db = db;
            _logger = logger;
            _audit = audit;
        }

        private Guid GetAdminId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(idStr, out var gid) ? gid : Guid.Empty;
        }

        [HttpPost]
        public async Task<ActionResult<QuestDto>> Create([FromBody] AdminQuestRequest req, CancellationToken ct)
        {
            if (req.Options is null || req.Options.Count() != 5)
                return BadRequest("Quest must have exactly 5 options.");

            if (req.CorrectOptionIndex < 0 || req.CorrectOptionIndex > 4)
                return BadRequest("CorrectOptionIndex must be 0..4.");

            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = req.QuestionText, // for EF Core migration compatibility
                QuestionText = req.QuestionText,
                Description = req.Description,
                Difficulty = req.Difficulty,
                SubjectCode = req.SubjectCode,
                BaseXp = req.BaseXp,
                IsPremiumOnly = req.IsPremiumOnly,
                CorrectOptionIndex = req.CorrectOptionIndex,
                CreatedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            int idx = 0;
            foreach (var o in req.Options)
            {
                quest.Options.Add(new QuestOption
                {
                    Id = Guid.NewGuid(),
                    QuestId = quest.Id,
                    Index = idx++,
                    Text = o
                });
            }

            _db.Quests.Add(quest);
            await _db.SaveChangesAsync(ct);

            // record admin action
            try
            {
                var adminId = GetAdminId();
                if (adminId != Guid.Empty)
                    await _audit.RecordAsync(adminId, "CreateQuest", $"Quest:{quest.Id}; Subject:{quest.SubjectCode}", null, ct);
            }
            catch { /* swallow audit errors */ }

            var dto = new QuestDto(quest.Id, quest.QuestionText, quest.Options.OrderBy(o=>o.Index).Select(o=>new QuestOptionDto(o.Index,o.Text)).ToList(), quest.BaseXp, quest.SubjectCode);
            return CreatedAtAction(nameof(GetById), new { id = quest.Id }, dto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestDto>>> List([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var q = await _db.Quests.Include(q=>q.Options).OrderByDescending(q=>q.CreatedAtUtc).Skip(offset).Take(limit).ToListAsync(ct);
            var list = q.Select(quest => new QuestDto(quest.Id, quest.QuestionText, quest.Options.OrderBy(o=>o.Index).Select(o=>new QuestOptionDto(o.Index,o.Text)).ToList(), quest.BaseXp, quest.SubjectCode)).ToList();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuestDto>> GetById(Guid id, CancellationToken ct)
        {
            var quest = await _db.Quests.Include(q=>q.Options).FirstOrDefaultAsync(q=>q.Id==id, ct);
            if (quest is null) return NotFound();
            var dto = new QuestDto(quest.Id, quest.QuestionText, quest.Options.OrderBy(o=>o.Index).Select(o=>new QuestOptionDto(o.Index,o.Text)).ToList(), quest.BaseXp, quest.SubjectCode);
            return Ok(dto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AdminQuestRequest req, CancellationToken ct)
        {
            var quest = await _db.Quests.Include(q=>q.Options).FirstOrDefaultAsync(q=>q.Id==id, ct);
            if (quest is null) return NotFound();

            if (req.Options is null || req.Options.Count() != 5)
                return BadRequest("Quest must have exactly 5 options.");

            if (req.CorrectOptionIndex < 0 || req.CorrectOptionIndex > 4)
                return BadRequest("CorrectOptionIndex must be 0..4.");

            quest.QuestionText = req.QuestionText;
            quest.Description = req.Description;
            quest.Difficulty = req.Difficulty;
            quest.SubjectCode = req.SubjectCode;
            quest.BaseXp = req.BaseXp;
            quest.IsPremiumOnly = req.IsPremiumOnly;
            quest.CorrectOptionIndex = req.CorrectOptionIndex;

            // replace options
            _db.QuestOptions.RemoveRange(quest.Options);
            int idx=0;
            foreach(var o in req.Options)
            {
                quest.Options.Add(new QuestOption{ Id=Guid.NewGuid(), QuestId=quest.Id, Index=idx++, Text=o});
            }

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var quest = await _db.Quests.FirstOrDefaultAsync(q=>q.Id==id, ct);
            if (quest is null) return NotFound();

            // soft-delete
            quest.IsActive = false;
            await _db.SaveChangesAsync(ct);
            try
            {
                var adminId = GetAdminId();
                if (adminId != Guid.Empty)
                    await _audit.RecordAsync(adminId, "DeleteQuest", $"Quest:{quest.Id}", null, ct);
            }
            catch { }
            return NoContent();
        }
    }
}
