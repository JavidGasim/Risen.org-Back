using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Universities
{
    public sealed record CandidatesResponse(
     int Limit,
     int Offset,
     int Total,
     IReadOnlyList<CandidateDto> Items
 );
}
