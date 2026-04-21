using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Administration;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/admin/subjects")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminSubjectsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminSubjectsController> _logger;
        private readonly Risen.Business.Services.Abstracts.IAdminAuditService _audit;

        public AdminSubjectsController(AppDbContext db, ILogger<AdminSubjectsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public AdminSubjectsController(AppDbContext db, ILogger<AdminSubjectsController> logger, Risen.Business.Services.Abstracts.IAdminAuditService audit)
        {
            _db = db;
            _logger = logger;
            _audit = audit;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Risen.Contracts.Administration.AdminSubjectRequest req, CancellationToken ct)
        {
            var code = req.Code.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(code)) return BadRequest("Code is required.");

            var exists = await _db.Subjects.AnyAsync(s=>s.Code==code, ct);
            if (exists) return Conflict("Subject with this code already exists.");

            var s = new Subject{ Code=code, Name=req.Name, Description=req.Description, IsActive=req.IsActive, CreatedAtUtc=DateTime.UtcNow };
            _db.Subjects.Add(s);
            await _db.SaveChangesAsync(ct);
            try
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (Guid.TryParse(idStr, out var adminId))
                {
                    await (_audit ?? throw new InvalidOperationException("Audit service not available")).RecordAsync(adminId, "CreateSubject", $"Subject:{s.Code}; Name:{s.Name}", null, ct);
                }
            }
            catch { }
            return CreatedAtAction(nameof(GetByCode), new { code = s.Code }, s);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subject>>> List(CancellationToken ct)
        {
            var items = await _db.Subjects.OrderBy(s=>s.Name).ToListAsync(ct);
            return Ok(items);
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<Subject>> GetByCode(string code, CancellationToken ct)
        {
            var s = await _db.Subjects.FirstOrDefaultAsync(x=>x.Code==code, ct);
            if (s is null) return NotFound();
            return Ok(s);
        }

        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, [FromBody] Risen.Contracts.Administration.AdminSubjectRequest req, CancellationToken ct)
        {
            var s = await _db.Subjects.FirstOrDefaultAsync(x=>x.Code==code, ct);
            if (s is null) return NotFound();
            s.Name = req.Name;
            s.Description = req.Description;
            s.IsActive = req.IsActive;
            await _db.SaveChangesAsync(ct);
            try
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (Guid.TryParse(idStr, out var adminId))
                {
                    await (_audit ?? throw new InvalidOperationException("Audit service not available")).RecordAsync(adminId, "UpdateSubject", $"Subject:{s.Code}; Name:{s.Name}", null, ct);
                }
            }
            catch { }
            return NoContent();
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code, CancellationToken ct)
        {
            var s = await _db.Subjects.FirstOrDefaultAsync(x=>x.Code==code, ct);
            if (s is null) return NotFound();
            s.IsActive = false; // soft delete
            await _db.SaveChangesAsync(ct);
            try
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (Guid.TryParse(idStr, out var adminId))
                {
                    await (_audit ?? throw new InvalidOperationException("Audit service not available")).RecordAsync(adminId, "DeleteSubject", $"Subject:{s.Code}", null, ct);
                }
            }
            catch { }
            return NoContent();
        }
    }
}
