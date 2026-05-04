using System;

namespace Risen.Contracts.Quests
{
    public sealed record QuestAttemptDto(
        Guid Id,
        Guid UserId,
        string? UserEmail,
        Guid QuestId,
        string? QuestTitle,
        Guid SelectedOptionId,
        bool IsCorrect,
        int AwardedXp,
        DateTime CompletedAtUtc,
        DateTime? CompletedDateUtc
    );
}
