using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Xp
{
    public sealed record AwardXpResponse(
    Guid TransactionId,
    int FinalXp,
    long NewTotalXp,
    string OldLeague,
    string NewLeague
);
}
