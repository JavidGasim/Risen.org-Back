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

        // ==================== ENTITLEMENTS ====================

        // GET /api/admin/plans/{planId:guid}/entitlements
        [HttpGet("{planId:guid}/entitlements")]
        public async Task<ActionResult> ListEntitlements(Guid planId, CancellationToken ct)
        {
            var plan = await _db.Plans.FindAsync(new object[] { planId }, ct);
            if (plan is null) return NotFound("Plan not found.");

            var entitlements = await _db.PlanEntitlements
                .AsNoTracking()
                .Where(e => e.PlanId == planId)
                .OrderBy(e => e.EntitlementKey)
                .Select(e => new AdminPlanEntitlementDto(
                    e.Id,
                    e.PlanId,
                    e.EntitlementKey,
                    e.EntitlementValue,
                    e.Description,
                    e.CreatedAtUtc,
                    e.UpdatedAtUtc
                ))
                .ToListAsync(ct);

            return Ok(entitlements);
        }

        // POST /api/admin/plans/{planId:guid}/entitlements
        [HttpPost("{planId:guid}/entitlements")]
        public async Task<ActionResult> CreateEntitlement(Guid planId, [FromBody] AdminPlanEntitlementRequest req, CancellationToken ct)
        {
            var plan = await _db.Plans.FindAsync(new object[] { planId }, ct);
            if (plan is null) return NotFound("Plan not found.");

            if (string.IsNullOrWhiteSpace(req.EntitlementKey))
                return BadRequest("EntitlementKey is required.");

            if (string.IsNullOrWhiteSpace(req.EntitlementValue))
                return BadRequest("EntitlementValue is required.");

            // Check if entitlement key already exists for this plan
            var exists = await _db.PlanEntitlements
                .AnyAsync(e => e.PlanId == planId && e.EntitlementKey == req.EntitlementKey, ct);
            if (exists)
                return Conflict("Entitlement with this key already exists for this plan.");

            var entitlement = new PlanEntitlement
            {
                Id = Guid.NewGuid(),
                PlanId = planId,
                EntitlementKey = req.EntitlementKey,
                EntitlementValue = req.EntitlementValue,
                Description = req.Description,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.PlanEntitlements.Add(entitlement);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Admin created entitlement {Key} for plan {PlanId}", req.EntitlementKey, planId);

            return CreatedAtAction(nameof(GetEntitlement), new { planId, id = entitlement.Id }, new AdminPlanEntitlementDto(
                entitlement.Id,
                entitlement.PlanId,
                entitlement.EntitlementKey,
                entitlement.EntitlementValue,
                entitlement.Description,
                entitlement.CreatedAtUtc,
                entitlement.UpdatedAtUtc
            ));
        }

        // GET /api/admin/plans/{planId:guid}/entitlements/{id:guid}
        [HttpGet("{planId:guid}/entitlements/{id:guid}")]
        public async Task<ActionResult> GetEntitlement(Guid planId, Guid id, CancellationToken ct)
        {
            var entitlement = await _db.PlanEntitlements.FirstOrDefaultAsync(e => e.Id == id && e.PlanId == planId, ct);
            if (entitlement is null) return NotFound();

            return Ok(new AdminPlanEntitlementDto(
                entitlement.Id,
                entitlement.PlanId,
                entitlement.EntitlementKey,
                entitlement.EntitlementValue,
                entitlement.Description,
                entitlement.CreatedAtUtc,
                entitlement.UpdatedAtUtc
            ));
        }

        // PUT /api/admin/plans/{planId:guid}/entitlements/{id:guid}
        [HttpPut("{planId:guid}/entitlements/{id:guid}")]
        public async Task<IActionResult> UpdateEntitlement(Guid planId, Guid id, [FromBody] AdminPlanEntitlementRequest req, CancellationToken ct)
        {
            var entitlement = await _db.PlanEntitlements.FirstOrDefaultAsync(e => e.Id == id && e.PlanId == planId, ct);
            if (entitlement is null) return NotFound();

            if (string.IsNullOrWhiteSpace(req.EntitlementValue))
                return BadRequest("EntitlementValue is required.");

            entitlement.EntitlementValue = req.EntitlementValue;
            entitlement.Description = req.Description;
            entitlement.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Admin updated entitlement {Key} for plan {PlanId}", entitlement.EntitlementKey, planId);

            return NoContent();
        }

        // DELETE /api/admin/plans/{planId:guid}/entitlements/{id:guid}
        [HttpDelete("{planId:guid}/entitlements/{id:guid}")]
        public async Task<IActionResult> DeleteEntitlement(Guid planId, Guid id, CancellationToken ct)
        {
            var entitlement = await _db.PlanEntitlements.FirstOrDefaultAsync(e => e.Id == id && e.PlanId == planId, ct);
            if (entitlement is null) return NotFound();

            _db.PlanEntitlements.Remove(entitlement);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Admin deleted entitlement {Key} for plan {PlanId}", entitlement.EntitlementKey, planId);

            return NoContent();
        }
    }
}