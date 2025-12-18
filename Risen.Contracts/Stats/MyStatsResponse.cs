using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Stats
{

    public sealed record MyStatsResponse(
        Guid UserId,
        string FirstName,
        string LastName,
        string FullName,
        string Email,
        string? UniversityName,
        DateTime? LastOnlineAtUtc,

        long TotalXp,
        string League,

        int CurrentStreak,
        int LongestStreak,
        DateTime? LastStreakDateUtc
    );
}
