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
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) => _config = config;

        public string CreateAccessToken(CustomIdentityUser user, IList<string> roles, bool isPremium, string plan)
        {
            var jwtKey = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "30");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("name", user.FullName ?? user.UserName ?? ""),
            new("plan", plan),
            new("entitlement", isPremium ? "premium" : "free")
        };

            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string Plain, string Hash, DateTime ExpiresAtUtc) CreateRefreshToken(int refreshDays)
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var plain = Convert.ToBase64String(bytes);

            var hash = HashToken(plain);
            var expires = DateTime.UtcNow.AddDays(refreshDays);

            return (plain, hash, expires);
        }

        public string HashToken(string token)
        {
            // token random olduğu üçün SHA256 kifayətdir
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes); // 64 hex chars
        }
    }
}