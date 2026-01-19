using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts;

public interface ITokenService
{
    string CreateAccessToken(CustomIdentityUser user, IList<string> roles, bool isPremium, string plan);
    (string Plain, string Hash, DateTime ExpiresAtUtc) CreateRefreshToken(int days);
    string HashToken(string token);
}
