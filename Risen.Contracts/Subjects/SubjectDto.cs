using System;

namespace Risen.Contracts.Subjects
{
    public sealed record SubjectDto(
        string Code,
        string Name,
        string? Description,
        bool IsActive
    );
}
