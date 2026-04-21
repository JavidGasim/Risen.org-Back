using System;

namespace Risen.Contracts.Administration
{
    public sealed record AdminSubjectRequest(
        string Code,
        string Name,
        string? Description = null,
        bool IsActive = true
    );
}
