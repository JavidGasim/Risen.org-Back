using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Contracts.Administration;
using Risen.DataAccess.Data;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/admin/xp-transactions
        [Authorize(Roles = "Admin")]
        [HttpGet("xp-transactions")]
        public async Task<ActionResult<IEnumerable<Risen.Contracts.Gamification.XpTransactionDto>>> GetXpTransactions(
            [FromQuery] Guid? userId = null,
            [FromQuery] Risen.Entities.Entities.XpSourceType? sourceType = null,
            [FromQuery] Guid? adminId = null,
            [FromQuery] DateTime? since = null,
            [FromQuery] int limit = 100,
            [FromQuery] int offset = 0,
            CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var q = _db.XpTransactions.AsNoTracking().AsQueryable();
            if (userId.HasValue) q = q.Where(x => x.UserId == userId.Value);
            if (sourceType.HasValue) q = q.Where(x => x.SourceType == sourceType.Value);
            if (adminId.HasValue) q = q.Where(x => x.AdminId == adminId.Value);
            if (since.HasValue) q = q.Where(x => x.CreatedAtUtc >= since.Value);

            var items = await q.OrderByDescending(x => x.CreatedAtUtc)
                .Skip(offset)
                .Take(limit)
                .Select(x => new Risen.Contracts.Gamification.XpTransactionDto(
                    x.Id, x.UserId, x.SourceType, x.SourceKey, x.BaseXp, x.DifficultyMultiplier, x.FinalXp, x.CreatedAtUtc, x.AdminId, x.AdminReason))
                .ToListAsync(ct);

            return Ok(items);
        }

        // GET /api/admin/actions?limit=50&offset=0
        [Authorize(Roles = "Admin")]
        [HttpGet("actions")]
        public async Task<ActionResult<IEnumerable<AdminActionDto>>> GetActions([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var q = _db.AdminActions.AsNoTracking()
                .OrderByDescending(a => a.CreatedAtUtc)
                .Skip(offset)
                .Take(limit)
                .Select(a => new AdminActionDto(a.Id, a.AdminId, a.TargetUserId, a.ActionType, a.Details, a.CreatedAtUtc));

            var items = await q.ToListAsync(ct);
            return Ok(items);
        }
    }
}
