using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Auth;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService auth, ILogger<AuthController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            await _auth.RegisterAsync(req, ct);
            _logger.LogInformation("New user registered: {Email}", req.Email);
            return Ok(new { message = "Registration completed" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var res = await _auth.LoginAsync(req, ct);
            _logger.LogInformation("User logged in: {Email}", req.Email);
            return Ok(res);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        {
            var res = await _auth.RefreshAsync(req, ct);
            _logger.LogInformation("Token refreshed for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));
            return Ok(res);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] Risen.Contracts.Auth.ForgotPasswordRequest req, CancellationToken ct)
        {
            var token = await _auth.SendForgotPasswordAsync(req, ct);
            // In Development we may return token to the caller for testing.
            if (HttpContext.RequestServices.GetService(typeof(IHostEnvironment)) is IHostEnvironment env && env.IsDevelopment())
            {
                return Ok(new { message = "If an account exists, a reset token was generated.", token });
            }

            return Ok(new { message = "If an account exists, a reset token was generated." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Risen.Contracts.Auth.ResetPasswordRequest req, CancellationToken ct)
        {
            var authRes = await _auth.ResetPasswordAsync(req, ct);
            if (HttpContext.RequestServices.GetService(typeof(IHostEnvironment)) is IHostEnvironment env && env.IsDevelopment())
            {
                return Ok(new { message = "Password has been reset.", auth = authRes });
            }

            return Ok(new { message = "Password has been reset." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
        {
            await _auth.LogoutAsync(req, ct);
            _logger.LogInformation("User logged out: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"));
            return Ok(new { message = "Logged out" });
        }
    }
}
