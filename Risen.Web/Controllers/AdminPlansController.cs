using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Administration;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/admin/plans")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPlansController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminPlansController> _logger;

        public AdminPlansController(AppDbContext db, ILogger<AdminPlansController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private Guid GetAdminId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(idStr, out var gid) ? gid : Guid.Empty;
        }

        private AdminPlanDto MapToDto(Plan p) => new AdminPlanDto(
            p.Id,
            p.Code.ToString(),
            p.Name,
            p.DailyQuestLimit,
            p.AllowAdvancedQuests,
            p.XpMultiplier,
            p.Description,
            p.CreatedAtUtc,
            p.UpdatedAtUtc
        );

        [HttpGet]
        public async Task<ActionResult> List(CancellationToken ct = default)
        {
            var items = await _db.Plans
    .AsNoTracking()
    .OrderBy(p => p.Name)
    .Select(p => new AdminPlanDto(
        p.Id,
        p.Code.ToString(),
        p.Name,
        p.DailyQuestLimit,
        p.AllowAdvancedQuests,
        p.XpMultiplier,
        p.Description,
        p.CreatedAtUtc,
        p.UpdatedAtUtc
    ))
    .ToListAsync(ct);
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AdminPlanRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Code)) return BadRequest("Code is required");
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required");

            if (await _db.Plans.AnyAsync(p => p.Name == req.Name || p.Code.ToString() == req.Code, ct))
                return Conflict("Plan with same code or name exists.");

            if (!Enum.TryParse<PlanCode>(req.Code, true, out var codeParsed))
                return BadRequest("Invalid plan code.");

            var plan = new Plan
            {
                Id = Guid.NewGuid(),
                Code = codeParsed,
                Name = req.Name,
                DailyQuestLimit = req.DailyQuestLimit,
                AllowAdvancedQuests = req.AllowAdvancedQuests,
                XpMultiplier = req.XpMultiplier,
                Description = req.Description,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.Plans.Add(plan);
            await _db.SaveChangesAsync(ct);

            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} created plan {PlanCode} with daily limit {DailyLimit}", adminId, req.Code, req.DailyQuestLimit);

            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, MapToDto(plan));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
        {
            var p = await _db.Plans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();
            return Ok(MapToDto(p));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AdminPlanRequest req, CancellationToken ct)
        {
            var p = await _db.Plans.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();

            if (!Enum.TryParse<PlanCode>(req.Code, true, out var codeParsed))
                return BadRequest("Invalid plan code.");

            p.Code = codeParsed;
            p.Name = req.Name;
            p.DailyQuestLimit = req.DailyQuestLimit;
            p.AllowAdvancedQuests = req.AllowAdvancedQuests;
            p.XpMultiplier = req.XpMultiplier;
            p.Description = req.Description;
            p.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} updated plan {PlanCode} daily limit to {DailyLimit}", adminId, req.Code, req.DailyQuestLimit);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var p = await _db.Plans.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null) return NotFound();

            _db.Plans.Remove(p);
            await _db.SaveChangesAsync(ct);

            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} deleted plan {PlanCode}", adminId, p.Code);

            return NoContent();
        }
    }
}