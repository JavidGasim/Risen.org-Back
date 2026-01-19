using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Risen.Business.Services.Abstracts;
using Risen.Entities.Entities;

namespace Risen.Business.Services.Concretes
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _cfg;

        public TokenService(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        public string CreateAccessToken(CustomIdentityUser user, IList<string> roles, bool isPremium, string plan)
        {
            var jwt = _cfg.GetSection("Jwt");
            var key = jwt["Key"]!;
            var issuer = jwt["Issuer"]!;
            var audience = jwt["Audience"]!;
            var minutes = int.Parse(jwt["AccessTokenMinutes"] ?? "30");

            var claims = new List<Claim>
        {
            // PRIMARY: user id (GUID)
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // Compatibility: many controllers read NameIdentifier
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("fullName", user.FullName ?? string.Empty),
            new Claim("plan", plan ?? "Free"),
            new Claim("isPremium", isPremium ? "true" : "false")
        };

            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string Plain, string Hash, DateTime ExpiresAtUtc) CreateRefreshToken(int days)
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var plain = Convert.ToBase64String(bytes);

            var hash = HashToken(plain);
            var exp = DateTime.UtcNow.AddDays(days);

            return (plain, hash, exp);
        }

        public string HashToken(string token)
        {
            // SHA256 hash (hex)
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}