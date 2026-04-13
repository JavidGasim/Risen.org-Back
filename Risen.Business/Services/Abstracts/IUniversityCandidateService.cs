using Risen.Contracts.Universities;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IUniversityCandidateService
    {
        Task<CandidatesResponse> GetCandidatesAsync(
            LeagueCode? minLeague,
            decimal? minScore,
            string? country,
            int limit,
            int offset,
            CancellationToken ct);
    }
}
