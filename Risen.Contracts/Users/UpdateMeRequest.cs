using System;

namespace Risen.Contracts.Users
{
    public sealed record UpdateMeRequest(
        string? FirstName,
        string? LastName,
        Guid? UniversityId,
        string? UniversityName
    );
}
