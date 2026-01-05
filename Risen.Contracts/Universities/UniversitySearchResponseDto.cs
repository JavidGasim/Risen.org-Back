using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Universities
{
    public sealed record UniversitySearchResponseDto(
     string Q,
     string? Country,
     int Limit,
     int Offset,
     IReadOnlyList<UniversityLocalDto> Local,
     IReadOnlyList<UniversitySearchItemDto> Items
 );
}
