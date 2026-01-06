using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Gamification
{
    public sealed record AwardXpResponse(
         int FinalXp,
         long NewTotalXp,
         string NewLeague
     );
}
