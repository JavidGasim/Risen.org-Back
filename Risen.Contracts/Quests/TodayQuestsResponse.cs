using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public sealed record TodayQuestsResponse(
       int DailyLimit,
       int CompletedToday,
       int RemainingToday,
       IReadOnlyList<TodayQuestDto> Items
   );
}
