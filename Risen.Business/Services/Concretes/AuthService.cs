using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
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

        public AuthService(
            UserManager<CustomIdentityUser> userManager,
            SignInManager<CustomIdentityUser> signInManager,
            RoleManager<CustomIdentityRole> roleManager,
            AppDbContext db,
            ITokenService tokenService,
            IEntitlementService entitlementService,
            IUniversityService universityService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
            _tokenService = tokenService;
            _entitlementService = entitlementService;
            _universityService = universityService;
        }

        public async Task RegisterAsync(RegisterRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var first = req.FirstName.Trim();
            var last = req.LastName.Trim();

            var exists = await _userManager.FindByEmailAsync(email);
            if (exists is not null)
                throw new InvalidOperationException("This email already exists.");

            var uniId = await _universityService.UpsertAndGetIdAsync(req.UniversityName, ct);

            var user = new CustomIdentityUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,

                FirstName = first,
                LastName = last,
                FullName = $"{first} {last}".Trim(),

                UniversityId = uniId,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

            // ---- UserStats: CreateAsync successful olduqdan sonra ----
            var rookieTierId = await _db.LeagueTiers.AsNoTracking()
                .Where(t => t.Code == LeagueCode.Rookie)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(ct);

            if (rookieTierId == Guid.Empty)
                throw new InvalidOperationException("League tiers are not seeded. Rookie tier not found.");

            _db.UserStats.Add(new UserStats
            {
                UserId = user.Id,
                TotalXp = 0,
                CurrentLeagueTierId = rookieTierId,
                CurrentStreak = 0,
                LongestStreak = 0,
                LastStreakDateUtc = null,
                UpdatedAtUtc = DateTime.UtcNow
            });

            // ---- Default role ----
            const string defaultRole = "Student";

            if (!await _roleManager.RoleExistsAsync(defaultRole))
            {
                var role = new CustomIdentityRole
                {
                    Id = Guid.NewGuid(),
                    Name = defaultRole,
                    NormalizedName = defaultRole.ToUpperInvariant()
                };

                var roleRes = await _roleManager.CreateAsync(role);
                if (!roleRes.Succeeded)
                    throw new InvalidOperationException(string.Join(" | ", roleRes.Errors.Select(e => e.Description)));
            }

            var addRoleRes = await _userManager.AddToRoleAsync(user, defaultRole);
            if (!addRoleRes.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", addRoleRes.Errors.Select(e => e.Description)));

            // ---- Default plan: Free ----
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
                throw new InvalidOperationException("Email or password is wrong.");

            var check = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
            if (!check.Succeeded)
                throw new InvalidOperationException("Email or password is wrong.");

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
                throw new InvalidOperationException("User couldn't find.");

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
