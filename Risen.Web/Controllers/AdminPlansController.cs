using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Administration;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;

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

        [HttpGet]
        public async Task<ActionResult> List(CancellationToken ct = default)
        {
            var items = await _db.Plans.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AdminPlanRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Code)) return BadRequest("Code is required");
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required");

            if (await _db.Plans.AnyAsync(p => p.Name == req.Name || p.Code.ToString() == req.Code, ct))
                return Conflict("Plan with same code or name exists.");

            // Try parse code to PlanCode enum
            if (!Enum.TryParse<PlanCode>(req.Code, true, out var codeParsed))
                return BadRequest("Invalid plan code.");

            var plan = new Plan { Id = Guid.NewGuid(), Code = codeParsed, Name = req.Name };
            _db.Plans.Add(plan);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
        {
            var p = await _db.Plans.FindAsync(new object[] { id }, ct);
            if (p is null) return NotFound();
            return Ok(p);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AdminPlanRequest req, CancellationToken ct)
        {
            var p = await _db.Plans.FindAsync(new object[] { id }, ct);
            if (p is null) return NotFound();
            if (!Enum.TryParse<PlanCode>(req.Code, true, out var codeParsed))
                return BadRequest("Invalid plan code.");

            p.Code = codeParsed;
            p.Name = req.Name;
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var p = await _db.Plans.FindAsync(new object[] { id }, ct);
            if (p is null) return NotFound();
            _db.Plans.Remove(p);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
