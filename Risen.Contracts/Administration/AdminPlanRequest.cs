using System;

namespace Risen.Contracts.Administration
{
    public sealed record AdminPlanRequest(
        string Code,
        string Name,
        int DailyQuestLimit,
        bool AllowAdvancedQuests,
        decimal XpMultiplier,
        string? Description
    );

    public sealed record AdminPlanDto(
        Guid Id,
        string Code,
        string Name,
        int DailyQuestLimit,
        bool AllowAdvancedQuests,
        decimal XpMultiplier,
        string? Description,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc
    );
}
