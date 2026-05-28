using System;

namespace Risen.Contracts.Administration
{
    public sealed record AdminPlanEntitlementDto(
        Guid Id,
        Guid PlanId,
        string EntitlementKey,
        string EntitlementValue,
        string? Description,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc
    );

    public sealed record AdminPlanEntitlementRequest(
        string EntitlementKey,
        string EntitlementValue,
        string? Description
    );
}
