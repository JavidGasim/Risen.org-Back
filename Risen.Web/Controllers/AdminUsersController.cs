using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<CustomIdentityUser> _users;
        private readonly RoleManager<CustomIdentityRole> _roles;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(AppDbContext db, UserManager<CustomIdentityUser> users, RoleManager<CustomIdentityRole> roles, ILogger<AdminUsersController> logger)
        {
            _db = db;
            _users = users;
            _roles = roles;
            _logger = logger;
        }
        private Guid GetAdminId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(idStr, out var gid) ? gid : Guid.Empty;
        }

        [HttpGet]
        public async Task<ActionResult> List([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var q = _db.Users.AsNoTracking().OrderBy(u => u.FullName).Skip(offset).Take(limit);
            var items = await q.Select(u => new { u.Id, u.FullName, u.Email, u.UniversityId }).ToListAsync(ct);
            return Ok(items);
        }

        [HttpPost("{id:guid}/roles")]
        public async Task<IActionResult> AddRole(Guid id, [FromBody] string role, CancellationToken ct)
        {
            var user = await _users.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            if (!await _roles.RoleExistsAsync(role))
            {
                var r = new CustomIdentityRole { Name = role };
                var ir = await _roles.CreateAsync(r);
                if (!ir.Succeeded) return BadRequest("Could not create role.");
            }

            var res = await _users.AddToRoleAsync(user, role);
            if (!res.Succeeded) return BadRequest(res.Errors);
            try
            {
                var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));
                _db.AdminActions.Add(new Risen.Entities.Entities.AdminAction
                {
                    Id = Guid.NewGuid(),
                    AdminId = adminId,
                    ActionType = "AddRole",
                    TargetUserId = user.Id,
                    Details = $"Role:{role}",
                    CreatedAtUtc = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(ct);
            }
            catch { }
            return NoContent();
        }

        [HttpDelete("{id:guid}/roles/{role}")]
        public async Task<IActionResult> RemoveRole(Guid id, string role, CancellationToken ct)
        {
            var user = await _users.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            var res = await _users.RemoveFromRoleAsync(user, role);
            if (!res.Succeeded) return BadRequest(res.Errors);
            try
            {
                var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));
                _db.AdminActions.Add(new Risen.Entities.Entities.AdminAction
                {
                    Id = Guid.NewGuid(),
                    AdminId = adminId,
                    ActionType = "RemoveRole",
                    TargetUserId = user.Id,
                    Details = $"Role:{role}",
                    CreatedAtUtc = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(ct);
            }
            catch { }
            return NoContent();
        }

        [HttpPut("{id:guid}/university/{uniId:guid}")]
        public async Task<IActionResult> SetUniversity(Guid id, Guid uniId, CancellationToken ct)
        {
            var user = await _users.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();

            var uni = await _db.Universities.FindAsync(new object[] { uniId }, ct);
            if (uni is null) return BadRequest("University not found.");

            user.UniversityId = uniId;
            await _users.UpdateAsync(user);
            try
            {
                var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));
                _db.AdminActions.Add(new Risen.Entities.Entities.AdminAction
                {
                    Id = Guid.NewGuid(),
                    AdminId = adminId,
                    ActionType = "SetUniversity",
                    TargetUserId = user.Id,
                    Details = $"University:{uniId}",
                    CreatedAtUtc = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(ct);
            }
            catch { }
            return NoContent();
        }

        [HttpDelete("{id:guid}/university")]
        public async Task<IActionResult> ClearUniversity(Guid id, CancellationToken ct)
        {
            var user = await _users.FindByIdAsync(id.ToString());
            if (user is null) return NotFound();
            user.UniversityId = null;
            await _users.UpdateAsync(user);
            try
            {
                var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));
                _db.AdminActions.Add(new Risen.Entities.Entities.AdminAction
                {
                    Id = Guid.NewGuid(),
                    AdminId = adminId,
                    ActionType = "ClearUniversity",
                    TargetUserId = user.Id,
                    Details = "",
                    CreatedAtUtc = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(ct);
            }
            catch { }
            return NoContent();
        }
    }
}
