using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Business.Services.Abstracts.Models;
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

        public AuthService(
            UserManager<CustomIdentityUser> userManager,
            SignInManager<CustomIdentityUser> signInManager,
            RoleManager<CustomIdentityRole> roleManager,
            AppDbContext db,
            ITokenService tokenService,
            IEntitlementService entitlementService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
            _tokenService = tokenService;
            _entitlementService = entitlementService;
        }

        public async Task RegisterAsync(RegisterRequest req, CancellationToken ct)
        {
            var exists = await _userManager.FindByEmailAsync(req.Email);
            if (exists is not null)
                throw new InvalidOperationException("Bu email artıq mövcuddur.");

            var user = new CustomIdentityUser
            {
                Id = Guid.NewGuid(),
                Email = req.Email,
                UserName = req.Email,
                FullName = req.FullName,
                Country = req.Country,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

            // Default role
            const string defaultRole = "Student";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
                await _roleManager.CreateAsync(new CustomIdentityRole { Id = Guid.NewGuid(), Name = defaultRole });

            await _userManager.AddToRoleAsync(user, defaultRole);

            // Default plan: Free (subscription yazmaq istəyirsənsə)
            var freePlanId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            _db.UserSubscriptions.Add(new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PlanId = freePlanId,
                IsActive = true,
                StartsAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user is null)
                throw new InvalidOperationException("Email və ya şifrə yanlışdır.");

            var check = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
            if (!check.Succeeded)
                throw new InvalidOperationException("Email və ya şifrə yanlışdır.");

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
            await _db.SaveChangesAsync(ct);

            return new AuthResponse(access, rt.Plain, plan, isPremium);
        }

        public async Task<AuthResponse> RefreshAsync(RefreshRequest req, CancellationToken ct)
        {
            var incomingHash = _tokenService.HashToken(req.RefreshToken);

            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == incomingHash, ct);
            if (stored is null || stored.IsRevoked || stored.ExpiresAtUtc <= DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token etibarsızdır.");

            var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
            if (user is null)
                throw new InvalidOperationException("User tapılmadı.");

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

            await _db.SaveChangesAsync(ct);

            return new AuthResponse(access, rt.Plain, plan, isPremium);
        }

        public async Task LogoutAsync(LogoutRequest req, CancellationToken ct)
        {
            var hash = _tokenService.HashToken(req.RefreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
            if (stored is null) return;

            stored.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
