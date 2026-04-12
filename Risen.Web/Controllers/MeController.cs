using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly ILogger<MeController> _logger;

        public MeController(ILogger<MeController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var plan = User.FindFirstValue("plan");
            var entitlement = User.FindFirstValue("entitlement");

            _logger.LogInformation("User {UserId} accessed /api/me with email {Email}, plan {Plan}, entitlement {Entitlement}", userId, email, plan, entitlement);

            return Ok(new { userId, email, plan, entitlement });
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
