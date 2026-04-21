using System;
using Risen.Entities.Entities;

namespace Risen.Contracts.Gamification
{
    public sealed record XpTransactionDto(
        Guid Id,
        Guid UserId,
        XpSourceType SourceType,
        string SourceKey,
        int BaseXp,
        decimal DifficultyMultiplier,
        int FinalXp,
        DateTime CreatedAtUtc,
        Guid? AdminId,
        string? AdminReason
    );
}
