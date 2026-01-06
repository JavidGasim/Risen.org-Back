using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Gamification
{
    public sealed record AwardXpRequest(
        XpSourceType SourceType,
        string SourceKey,
        int BaseXp,
        decimal DifficultyMultiplier = 1.0m
    );
}
