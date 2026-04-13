using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Universities
{
    public sealed record CandidateDto(
    Guid UserId,
    string FullName,
    string? UniversityName,
    string? Country,
    string LeagueCode,
    string LeagueName,
    long TotalXp,
    decimal RisenScore,
    int CurrentStreak,
    int LongestStreak
);
}
