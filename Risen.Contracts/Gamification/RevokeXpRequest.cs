using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Gamification
{
    public sealed record RevokeXpRequest(
        Guid TargetUserId,
        string OriginalSourceKey,
        string Reason
    );
}
