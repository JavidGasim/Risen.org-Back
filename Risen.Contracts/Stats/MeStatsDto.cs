using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Stats
{
    public sealed record MeStatsDto(
     Guid UserId,
        string FirstName,
        string LastName,
        string FullName,
        string Email,
        string? UniversityName,

        long TotalXp,
        string LeagueCode,
        string LeagueName,

        int CurrentStreak,
        int LongestStreak,
        DateTime? LastStreakDateUtc,

        DateTime CreatedAtUtc,
        DateTime? LastOnlineAtUtc,

        string Plan,
        bool IsPremium
);
}
