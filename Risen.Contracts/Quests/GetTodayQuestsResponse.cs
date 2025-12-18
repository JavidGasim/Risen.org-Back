using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public sealed record GetTodayQuestsResponse(
    int DailyLimit,
    int CompletedTodayCount,
    int RemainingToday,
    IReadOnlyList<TodayQuestDto> Items
);
}
