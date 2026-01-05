using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Leaderboards
{
    public sealed record LeaderboardResponse(
    int Limit,
    int Offset,
    IReadOnlyList<LeaderboardEntryDto> Items,
    int Total = 0 // default: köhnə çağırışlar qırılmasın
  );
}
