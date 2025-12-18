using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Leaderboards
{
    public sealed record LeaderboardEntryDto(
    int Rank,
    Guid UserId,
    string DisplayName,
    long TotalXp,
    string League,
    string? UniversityName
);
}
