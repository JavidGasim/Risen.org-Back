using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Contracts.Universities
{
    public sealed record UniversitySearchItemDto(
    string Name,
    string? Country,
    string? StateProvince,
    string? Domain,
    string? WebPage
);
}
