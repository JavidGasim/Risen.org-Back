using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public record SubmitQuestAnswerRequest(int SelectedOptionIndex);

    public record SubmitQuestAnswerResponse(
        bool IsCorrect,
        int? CorrectOptionIndex,
        int EarnedXp
    );
}
