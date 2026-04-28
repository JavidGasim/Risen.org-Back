using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly ILogger<MeController> _logger;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly AppDbContext _db;
        private readonly Risen.Business.Services.Abstracts.IEntitlementService _entitlement;
        private readonly Risen.Business.Services.Abstracts.IUniversityService _uniService;

        public MeController(ILogger<MeController> logger, UserManager<CustomIdentityUser> userManager, AppDbContext db, Risen.Business.Services.Abstracts.IEntitlementService entitlement, Risen.Business.Services.Abstracts.IUniversityService uniService)
        {
            _logger = logger;
            _userManager = userManager;
            _db = db;
            _entitlement = entitlement;
            _uniService = uniService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct = default)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(idStr))
            {
                _logger.LogWarning("User id claim is missing in the token.");
                return Unauthorized("User id claim is missing.");
            }

            var userId = Guid.Parse(idStr);

            var user = await _db.Users.AsNoTracking()
                .Include(u => u.University)
                .Include(u => u.Stats)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found in database.", userId);
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var (isPremium, plan) = await _entitlement.GetUserEntitlementAsync(user.Id, ct);

            var dto = new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.FullName,
                user.CreatedAtUtc,
                user.LastOnlineAtUtc,
                Roles = roles,
                Entitlement = new { IsPremium = isPremium, Plan = plan },
                University = user.University is null ? null : new { user.University.Id, user.University.Name, user.University.Country },
                Stats = user.Stats is null ? null : new
                {
                    user.Stats.TotalXp,
                    user.Stats.CurrentStreak,
                    user.Stats.LongestStreak,
                    LastStreakDateUtc = user.Stats.LastStreakDateUtc,
                    user.Stats.RisenScore,
                    user.Stats.CurrentLeagueTierId
                }
            };

            _logger.LogInformation("User {UserId} accessed /api/me", userId);
            return Ok(dto);
        }

        // PUT /api/me
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Risen.Contracts.Users.UpdateMeRequest req, CancellationToken ct = default)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(idStr))
                return Unauthorized();

            var userId = Guid.Parse(idStr);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return NotFound();

            var changed = false;

            if (!string.IsNullOrWhiteSpace(req.FirstName) && req.FirstName != user.FirstName)
            {
                user.FirstName = req.FirstName.Trim();
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(req.LastName) && req.LastName != user.LastName)
            {
                user.LastName = req.LastName.Trim();
                changed = true;
            }

            if (changed)
            {
                user.FullName = $"{user.FirstName} {user.LastName}".Trim();
            }

            // university can be set by id or name
            if (req.UniversityId.HasValue)
            {
                user.UniversityId = req.UniversityId;
            }
            else if (!string.IsNullOrWhiteSpace(req.UniversityName))
            {
                var uniId = await _uniService.UpsertAndGetIdAsync(req.UniversityName.Trim(), ct);
                user.UniversityId = uniId;
            }

            await _userManager.UpdateAsync(user);

            // return updated profile
            return await Get(ct);
        }

        [Authorize(Policy = "PremiumOnly")]
        [HttpGet("premium-area")]
        public IActionResult PremiumArea()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            _logger.LogInformation("User {UserId} accessed the premium area.", userId);
            return Ok(new { message = "Premium endpoint works." });
        }
    }
}
