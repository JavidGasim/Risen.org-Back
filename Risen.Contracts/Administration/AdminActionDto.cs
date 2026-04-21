using System;

namespace Risen.Contracts.Administration
{
    public sealed record AdminActionDto(
        Guid Id,
        Guid AdminId,
        Guid? TargetUserId,
        string ActionType,
        string Details,
        DateTime CreatedAtUtc
    );
}
