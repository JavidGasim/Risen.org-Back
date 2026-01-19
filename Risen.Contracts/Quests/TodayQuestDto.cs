using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Quests
{
    public record TodayQuestDto(
     Guid Id,
     string Title,
     int XpReward,
     bool IsCompletedToday,
     List<QuestOptionDto> Options // yalnız əgər listdən oynadırsansa
 );

}
