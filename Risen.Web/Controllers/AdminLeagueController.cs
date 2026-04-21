using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Administration;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/admin/league-tiers")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminLeagueController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminLeagueController> _logger;

        private readonly Risen.Business.Services.Abstracts.IAdminAuditService _audit;

        public AdminLeagueController(AppDbContext db, ILogger<AdminLeagueController> logger, Risen.Business.Services.Abstracts.IAdminAuditService audit)
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminLeagueTierDto>>> List(CancellationToken ct = default)
        {
            var tiers = await _db.LeagueTiers.OrderBy(t => t.SortOrder).ToListAsync(ct);
            var dto = tiers.Select(t => new AdminLeagueTierDto(t.Id, t.Code, t.Name, t.MinXp, t.MaxXp, t.SortOrder, t.Weight)).ToList();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<AdminLeagueTierDto>> Create([FromBody] AdminLeagueTierRequest req, CancellationToken ct)
        {
            // ensure code uniqueness
            if (await _db.LeagueTiers.AnyAsync(t => t.Code == req.Code, ct))
                return Conflict("League code already exists.");

            var tier = new LeagueTier
            {
                Id = Guid.NewGuid(),
                Code = req.Code,
                Name = req.Name,
                MinXp = req.MinXp,
                MaxXp = req.MaxXp,
                SortOrder = req.SortOrder,
                Weight = req.Weight
            };

            _db.LeagueTiers.Add(tier);
            await _db.SaveChangesAsync(ct);

            try
            {
                var adminId = GetAdminId();
                if (adminId != Guid.Empty)
                    await _audit.RecordAsync(adminId, "CreateLeagueTier", $"Tier:{tier.Code}; Min:{tier.MinXp}; Max:{tier.MaxXp}", null, ct);
            }
            catch { }

            var dto = new AdminLeagueTierDto(tier.Id, tier.Code, tier.Name, tier.MinXp, tier.MaxXp, tier.SortOrder, tier.Weight);
            return CreatedAtAction(nameof(GetById), new { id = tier.Id }, dto);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AdminLeagueTierDto>> GetById(Guid id, CancellationToken ct)
        {
            var t = await _db.LeagueTiers.FindAsync(new object[] { id }, ct);
            if (t is null) return NotFound();
            return Ok(new AdminLeagueTierDto(t.Id, t.Code, t.Name, t.MinXp, t.MaxXp, t.SortOrder, t.Weight));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AdminLeagueTierRequest req, CancellationToken ct)
        {
            var t = await _db.LeagueTiers.FindAsync(new object[] { id }, ct);
            if (t is null) return NotFound();

            t.Code = req.Code;
            t.Name = req.Name;
            t.MinXp = req.MinXp;
            t.MaxXp = req.MaxXp;
            t.SortOrder = req.SortOrder;
            t.Weight = req.Weight;

            await _db.SaveChangesAsync(ct);
            try
            {
                var adminId = GetAdminId();
                if (adminId != Guid.Empty)
                    await _audit.RecordAsync(adminId, "UpdateLeagueTier", $"Tier:{t.Code}; Min:{t.MinXp}; Max:{t.MaxXp}", null, ct);
            }
            catch { }
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var t = await _db.LeagueTiers.FindAsync(new object[] { id }, ct);
            if (t is null) return NotFound();

            _db.LeagueTiers.Remove(t);
            await _db.SaveChangesAsync(ct);
            try
            {
                var adminId = GetAdminId();
                if (adminId != Guid.Empty)
                    await _audit.RecordAsync(adminId, "DeleteLeagueTier", $"Tier:{t.Code}", null, ct);
            }
            catch { }
            return NoContent();
        }
    }
}
