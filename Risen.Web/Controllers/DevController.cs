using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Risen.Entities.Entities;
using Microsoft.Extensions.Hosting;

namespace Risen.Web.Controllers
{
    [Route("api/dev")]
    [ApiController]
    public class DevController : ControllerBase
    {
        private readonly UserManager<CustomIdentityUser> _users;
        private readonly RoleManager<CustomIdentityRole> _roles;
        private readonly IHostEnvironment _env;
        private readonly ILogger<DevController> _logger;

        public DevController(UserManager<CustomIdentityUser> users, RoleManager<CustomIdentityRole> roles, IHostEnvironment env, ILogger<DevController> logger)
        {
            _users = users;
            _roles = roles;
            _env = env;
            _logger = logger;
        }

        public sealed record SeedAdminRequest(string Email, string Password, string FirstName, string LastName);

        [HttpPost("seed-admin")]
        public async Task<IActionResult> SeedAdmin([FromBody] SeedAdminRequest req)
        {
            if (!_env.IsDevelopment())
                return Forbid();

            var email = req.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and Password are required.");

            // ensure roles
            foreach (var role in new[] { "Admin", "Student", "University" })
            {
                if (!await _roles.RoleExistsAsync(role))
                {
                    var r = new CustomIdentityRole { Id = Guid.NewGuid(), Name = role };
                    var cr = await _roles.CreateAsync(r);
                    if (!cr.Succeeded)
                    {
                        _logger.LogWarning("Could not create role {Role}: {Errors}", role, string.Join(',', cr.Errors.Select(e=>e.Description)));
                    }
                }
            }

            var user = await _users.FindByEmailAsync(email);
            if (user is null)
            {
                user = new CustomIdentityUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    FirstName = req.FirstName ?? "Admin",
                    LastName = req.LastName ?? "User",
                    FullName = $"{req.FirstName} {req.LastName}".Trim(),
                    CreatedAtUtc = DateTime.UtcNow
                };

                var createRes = await _users.CreateAsync(user, req.Password);
                if (!createRes.Succeeded)
                {
                    return BadRequest(new { message = "Could not create admin user", errors = createRes.Errors.Select(e => e.Description) });
                }
            }
            else
            {
                // reset password
                var token = await _users.GeneratePasswordResetTokenAsync(user);
                var reset = await _users.ResetPasswordAsync(user, token, req.Password);
                if (!reset.Succeeded)
                {
                    return BadRequest(new { message = "Could not reset password", errors = reset.Errors.Select(e => e.Description) });
                }

                // ensure names
                user.FirstName = req.FirstName ?? user.FirstName;
                user.LastName = req.LastName ?? user.LastName;
                user.FullName = string.IsNullOrWhiteSpace(user.FullName) ? $"{user.FirstName} {user.LastName}".Trim() : user.FullName;
                await _users.UpdateAsync(user);
            }

            // ensure admin role
            if (!await _users.IsInRoleAsync(user, "Admin"))
            {
                await _users.AddToRoleAsync(user, "Admin");
            }

            return Ok(new { message = "Admin seeded", email = user.Email });
        }
    }
}
