using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Options
{
    public class QuestPolicyOptions
    {
        public int FreeDailyQuestLimit { get; set; } = 10;
        public int PremiumDailyQuestLimit { get; set; } = 1000;

        public decimal NormalMultiplier { get; set; } = 1.0m;
        public decimal AdvancedMultiplier { get; set; } = 1.5m;

        public int StreakBonusXp { get; set; } = 20;

    }
}
