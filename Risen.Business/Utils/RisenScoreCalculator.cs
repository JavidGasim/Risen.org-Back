using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Utils
{
    public static class RisenScoreCalculator
    {
        public static decimal Calculate(int leagueWeight, long totalXp, int streakDays)
        {
            var score = leagueWeight + totalXp / 150m + streakDays * 0.5m;
            return Math.Round(score, 2);
        }
    }
}
