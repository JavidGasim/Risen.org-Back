using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public sealed record QuestListItemDto(
     Guid Id,
     string Title,
     string? Description, // nullable
     QuestDifficulty Difficulty,
     int BaseXp,
     bool IsPremiumOnly,
     bool IsCompletedToday,
     bool IsCompletedEver
 );
}
