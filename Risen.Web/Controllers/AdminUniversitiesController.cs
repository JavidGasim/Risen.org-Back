using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;

namespace Risen.Web.Controllers
{
    [Route("api/admin/universities")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUniversitiesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminUniversitiesController> _logger;

        public AdminUniversitiesController(AppDbContext db, ILogger<AdminUniversitiesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<University>>> List([FromQuery] int limit = 100, [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);
            var items = await _db.Universities.OrderBy(u=>u.Name).Skip(offset).Take(limit).ToListAsync(ct);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<University>> GetById(Guid id, CancellationToken ct)
        {
            var u = await _db.Universities.FindAsync(new object[] { id }, ct);
            if (u is null) return NotFound();
            return Ok(u);
        }

        [HttpPost]
        public async Task<ActionResult<University>> Create([FromBody] University req, CancellationToken ct)
        {
            // minimal validation
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
            req.Id = Guid.NewGuid();
            req.NormalizedKey = (req.Name ?? "").Trim().ToLowerInvariant();
            req.CreatedAtUtc = DateTime.UtcNow;
            _db.Universities.Add(req);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetById), new { id = req.Id }, req);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] University req, CancellationToken ct)
        {
            var u = await _db.Universities.FindAsync(new object[] { id }, ct);
            if (u is null) return NotFound();
            u.Name = req.Name;
            u.NormalizedKey = (req.Name ?? "").Trim().ToLowerInvariant();
            u.Country = req.Country;
            u.StateProvince = req.StateProvince;
            u.PrimaryDomain = req.PrimaryDomain;
            u.PrimaryWebPage = req.PrimaryWebPage;
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var u = await _db.Universities.FindAsync(new object[] { id }, ct);
            if (u is null) return NotFound();
            _db.Universities.Remove(u);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
