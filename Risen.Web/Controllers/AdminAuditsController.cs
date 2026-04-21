using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.DataAccess.Data;
using System.Text;

namespace Risen.Web.Controllers
{
    [Route("api/admin/audit")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAuditsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminAuditsController> _logger;

        public AdminAuditsController(AppDbContext db, ILogger<AdminAuditsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET /api/admin/audit/export?since=&until=&adminId=&actionType=
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] DateTime? since = null, [FromQuery] DateTime? until = null, [FromQuery] Guid? adminId = null, [FromQuery] string? actionType = null, CancellationToken ct = default)
        {
            var q = _db.AdminActions.AsNoTracking().AsQueryable();
            if (since.HasValue) q = q.Where(a => a.CreatedAtUtc >= since.Value);
            if (until.HasValue) q = q.Where(a => a.CreatedAtUtc <= until.Value);
            if (adminId.HasValue) q = q.Where(a => a.AdminId == adminId.Value);
            if (!string.IsNullOrWhiteSpace(actionType)) q = q.Where(a => a.ActionType == actionType);

            var list = await q.OrderByDescending(a => a.CreatedAtUtc).ToListAsync(ct);

            var sb = new StringBuilder();
            sb.AppendLine("Id,AdminId,TargetUserId,ActionType,Details,CreatedAtUtc");
            foreach (var a in list)
            {
                var detailsEsc = a.Details?.Replace("\"", "\"\"") ?? string.Empty;
                sb.AppendLine($"\"{a.Id}\",\"{a.AdminId}\",\"{a.TargetUserId?.ToString() ?? string.Empty}\",\"{a.ActionType}\",\"{detailsEsc}\",\"{a.CreatedAtUtc:o}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"admin-audit-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}
