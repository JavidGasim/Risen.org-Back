using System;
using Risen.Entities.Entities;

namespace Risen.Contracts.Administration
{
    public sealed record AdminLeagueTierDto(
        Guid Id,
        LeagueCode Code,
        string Name,
        long MinXp,
        long? MaxXp,
        int SortOrder,
        int Weight
    );

    public sealed record AdminLeagueTierRequest(
        LeagueCode Code,
        string Name,
        long MinXp,
        long? MaxXp,
        int SortOrder,
        int Weight
    );
}
