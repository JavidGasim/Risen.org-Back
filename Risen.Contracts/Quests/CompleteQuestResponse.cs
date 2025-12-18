using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public sealed record CompleteQuestResponse(
      int AwardedXp,
      long TotalXp,
      string League,
      int CurrentStreak,
      int LongestStreak
  );
}
