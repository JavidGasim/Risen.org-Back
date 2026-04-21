using System;

namespace Risen.Contracts.Administration
{
    public sealed record AdminPlanRequest(
        string Code,
        string Name
    );
}
