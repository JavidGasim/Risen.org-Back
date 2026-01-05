using Risen.Contracts.Universities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IUniversitySuggestService
    {
        Task<string[]> SuggestAsync(string q, int limit, CancellationToken ct);
        Task<UniversitySearchResponseDto> SearchAsync(string q, string? country, int limit, int offset, CancellationToken ct);
    }
}
