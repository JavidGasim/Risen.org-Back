using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public record QuestDto(
    Guid Id,
    string QuestionText,
    List<QuestOptionDto> Options,
    int XpReward
);

    public record QuestOptionDto(int Index, string Text);
}
