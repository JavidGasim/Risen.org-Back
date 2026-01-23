using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public sealed record SubmitQuestAnswerRequest(
        Guid QuestId,
        int SelectedIndex // 0..4
    );

    public sealed record SubmitQuestAnswerResponse(
         bool IsCorrect,
         int CorrectIndex,
         int AwardedXp,
         long TotalXp,
         string League,
         int CurrentStreak,
         int LongestStreak
     );
}
