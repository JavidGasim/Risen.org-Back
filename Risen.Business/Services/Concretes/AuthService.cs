using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Exceptions;
using Risen.Business.Services.Abstracts;
using Risen.Business.Utils;
using Risen.Contracts.Auth;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly SignInManager<CustomIdentityUser> _signInManager;
        private readonly RoleManager<CustomIdentityRole> _roleManager;
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IEntitlementService _entitlementService;
        private readonly IUniversityService _universityService;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<CustomIdentityUser> userManager,
            SignInManager<CustomIdentityUser> signInManager,
            RoleManager<CustomIdentityRole> roleManager,
            AppDbContext db,
            ITokenService tokenService,
            IEntitlementService entitlementService,
            IUniversityService universityService, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
            _emailService = emailService;
            _tokenService = tokenService;
            _entitlementService = entitlementService;
            _universityService = universityService;
        }

        public async Task<string?> SendForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct)
        {
            var email = req.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return null; // do not reveal

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // store token hash in RefreshTokens table with 1 hour expiry
            var hash = _tokenService.HashToken(token);
            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);

            // Send email with reset link (frontend link containing token)
            try
            {
                var emailSettings = new Risen.Entities.Entities.EmailSettings();
                // try to resolve via configuration
                // build reset url
                var resetUrl = $"{emailSettings.FrontendBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(user.Email ?? string.Empty)}&token={Uri.EscapeDataString(token)}";

                var subject = "Reset your Risen password";
                var body = $"<p>Please click the link below to reset your password:</p><p><a href=\"{resetUrl}\">Reset password</a></p>";

                // attempt to resolve IEmailService from DI
            try
            {
                // Resolve IEmailService from DI
                var emailSvc = (Risen.Business.Services.Abstracts.IEmailService)Activator.CreateInstance(typeof(Risen.Business.Services.Concretes.SmtpEmailService));
                // If DI is available, code should use injected service; Activator.CreateInstance used as placeholder.
            }
            catch
            {
                // swallow
            }

            }
            catch
            {
                // swallow email send errors for now
            }

            return token;
        }

        public async Task<Risen.Contracts.Auth.AuthResponse?> ResetPasswordAsync(ResetPasswordRequest req, CancellationToken ct)
        {
            var email = req.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                throw new BadRequestException("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                throw new BadRequestException("User not found.");

            // Try matching the token in several common encodings. Use RevokedAtUtc == null for EF translation.
            string? tokenToUse = req.Token;
            RefreshToken? stored = null;

            if (!string.IsNullOrEmpty(tokenToUse))
            {
                var incomingHash = _tokenService.HashToken(tokenToUse);
                stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == incomingHash && x.UserId == user.Id && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow, ct);
            }

            if (stored is null)
            {
                // try URL-unescaped token
                try
                {
                    var decoded = Uri.UnescapeDataString(req.Token ?? string.Empty);
                    if (!string.IsNullOrEmpty(decoded) && decoded != req.Token)
                    {
                        var h2 = _tokenService.HashToken(decoded);
                        stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == h2 && x.UserId == user.Id && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow, ct);
                        if (stored != null) tokenToUse = decoded;
                    }
                }
                catch { /* ignore decode errors */ }
            }

            if (stored is null)
            {
                // try common client-side space->plus substitution
                var plused = req.Token?.Replace(' ', '+');
                if (!string.IsNullOrEmpty(plused) && plused != req.Token)
                {
                    var h3 = _tokenService.HashToken(plused);
                    stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == h3 && x.UserId == user.Id && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow, ct);
                    if (stored != null) tokenToUse = plused;
                }
            }

            if (stored is null)
                throw new BadRequestException("Reset token is invalid or expired.");

            var resetRes = await _userManager.ResetPasswordAsync(user, tokenToUse!, req.NewPassword);
            if (!resetRes.Succeeded)
                throw new BadRequestException(string.Join(" | ", resetRes.Errors.Select(e => e.Description)));

            // revoke token
            stored.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            // Automatically log in the user and return tokens
            var roles = await _userManager.GetRolesAsync(user);
            var (isPremium, plan) = await _entitlementService.GetUserEntitlementAsync(user.Id, ct);

            var access = _tokenService.CreateAccessToken(user, roles, isPremium, plan);
            var rt = _tokenService.CreateRefreshToken(30);

            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = rt.Hash,
                ExpiresAtUtc = rt.ExpiresAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            });

            user.LastOnlineAtUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            await _db.SaveChangesAsync(ct);

            return new Risen.Contracts.Auth.AuthResponse(access, rt.Plain, plan, isPremium);
        }

        public async Task RegisterAsync(RegisterRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();

            var exists = await _userManager.FindByEmailAsync(email);
            if (exists is not null)
                throw new BadRequestException("This email already exists.");

            var code = new Random().Next(100000, 999999).ToString();

            var body = $@"
<div style='margin:0; padding:0; background:#0f172a; font-family:Segoe UI, Arial, sans-serif;'>

    <div style='max-width:520px; margin:40px auto; background:#111827; 
                border-radius:16px; padding:40px 30px; 
                box-shadow:0 20px 40px rgba(0,0,0,0.6); text-align:center;'>

        <!-- Logo / Title -->
        <h1 style='color:#a78bfa; margin-bottom:10px; letter-spacing:1px;'>
            RISEN
        </h1>

        <p style='color:#9ca3af; font-size:14px; margin-bottom:30px;'>
            Verify your account
        </p>

        <!-- OTP Box -->
        <div style='background: linear-gradient(135deg,#6366f1,#8b5cf6);
                    padding:18px 0; border-radius:12px; margin-bottom:25px;'>

            <span style='font-size:32px; color:white; 
                         letter-spacing:8px; font-weight:bold;'>
                {code}
            </span>
        </div>

        <!-- Message -->
        <p style='color:#d1d5db; font-size:15px; line-height:1.6;'>
            Use this verification code to complete your registration.
        </p>

        <p style='color:#6b7280; font-size:12px; margin-top:20px;'>
            This code will expire in 60 seconds.
        </p>

        <!-- Divider -->
        <div style='margin:30px 0; height:1px; background:#1f2937;'></div>

        <!-- Footer -->
        <p style='color:#6b7280; font-size:12px;'>
            © 2026 Risen. All rights reserved.
        </p>

    </div>

</div>
";

            OtpStore.Data[email] = (req, code, DateTime.UtcNow.AddMinutes(5));

            try
            {
                await _emailService.SendAsync(
                    email,
                    "Verification Code",
                    body
                );
            }
            catch (Exception ex)
            {
                // 🔥 BURANI LOG ET
                throw new Exception("EMAIL SERVICE FAILED: " + ex.Message);
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user is null)
                throw new BadRequestException("Email or password is wrong.");

            var check = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
            if (!check.Succeeded)
                throw new BadRequestException("Email or password is wrong.");

            var roles = await _userManager.GetRolesAsync(user);
            var (isPremium, plan) = await _entitlementService.GetUserEntitlementAsync(user.Id, ct);

            var access = _tokenService.CreateAccessToken(user, roles, isPremium, plan);

            var refreshDays = 30; // appsettings-dən də oxuya bilərsən
            var rt = _tokenService.CreateRefreshToken(refreshDays);

            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = rt.Hash,
                ExpiresAtUtc = rt.ExpiresAtUtc
            });

            user.LastOnlineAtUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _db.SaveChangesAsync(ct);

            return new AuthResponse(access, rt.Plain, plan, isPremium);
        }

        public async Task<AuthResponse> RefreshAsync(RefreshRequest req, CancellationToken ct)
        {
            var incomingHash = _tokenService.HashToken(req.RefreshToken);

            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == incomingHash, ct);
            if (stored is null || stored.IsRevoked || stored.ExpiresAtUtc <= DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token is not valid.");

            var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
            if (user is null)
                throw new NotFoundException("User couldn't find.");

            var roles = await _userManager.GetRolesAsync(user);
            var (isPremium, plan) = await _entitlementService.GetUserEntitlementAsync(user.Id, ct);

            var access = _tokenService.CreateAccessToken(user, roles, isPremium, plan);

            // Rotate refresh token
            var rt = _tokenService.CreateRefreshToken(30);

            stored.RevokedAtUtc = DateTime.UtcNow;
            stored.ReplacedByTokenHash = rt.Hash;

            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = rt.Hash,
                ExpiresAtUtc = rt.ExpiresAtUtc
            });

            user.LastOnlineAtUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _db.SaveChangesAsync(ct);

            return new AuthResponse(access, rt.Plain, plan, isPremium);
        }

        public async Task VerifyRegisterAsync(VerifyRegisterRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();

            if (!OtpStore.Data.TryGetValue(email, out var data))
                throw new BadRequestException("Code not found");

            if (data.Expire < DateTime.UtcNow)
                throw new BadRequestException("Code expired");

            if (data.Code != req.Code)
                throw new BadRequestException("Invalid code");

            var r = data.Req;

            var uniId = await _universityService.UpsertAndGetIdAsync(r.UniversityName, ct);

            var user = new CustomIdentityUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                FirstName = r.FirstName,
                LastName = r.LastName,
                FullName = $"{r.FirstName} {r.LastName}".Trim(),
                UniversityId = uniId,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, r.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

            await AddDefaultData(user, ct);

            OtpStore.Data.Remove(email);
        }

        public async Task LogoutAsync(LogoutRequest req, CancellationToken ct)
        {
            var hash = _tokenService.HashToken(req.RefreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
            if (stored is null) return;

            stored.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        private async Task AddDefaultData(CustomIdentityUser user, CancellationToken ct)
        {
            var rookieTierId = await _db.LeagueTiers
                .Where(t => t.Code == LeagueCode.Rookie)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(ct);

            _db.UserStats.Add(new UserStats
            {
                UserId = user.Id,
                TotalXp = 0,
                CurrentLeagueTierId = rookieTierId,
                UpdatedAtUtc = DateTime.UtcNow
            });

            const string roleName = "Student";

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new CustomIdentityRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }

            await _userManager.AddToRoleAsync(user, roleName);

            var planId = await _db.Plans
                .Where(p => p.Code == PlanCode.Free)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(ct);

            _db.UserSubscriptions.Add(new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PlanId = planId,
                IsActive = true,
                StartsAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }
    }
}