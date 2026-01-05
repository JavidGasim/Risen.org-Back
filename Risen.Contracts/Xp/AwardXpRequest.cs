using Risen.Entities.Entities;

namespace Risen.Contracts.Xp
{
    public sealed record AwardXpRequest(
    XpSourceType SourceType,
    string SourceKey,
    int BaseXp,
    decimal DifficultyMultiplier = 1.0m
);
}
