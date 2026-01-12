using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public sealed record TodayQuestDto(
           Guid Id,
        string Title,
        string SubjectCode,
        string Difficulty,
        int BaseXp,
        bool IsCompletedToday
  );
}
