using Risen.Contracts.Leaderboards;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface ILeaderboardService
    {
        Task<LeaderboardResponse> GetGlobalAsync(LeagueCode? league, int limit, int offset, CancellationToken ct);
        Task<LeaderboardResponse> GetUniversityAsync(Guid universityId, LeagueCode? league, int limit, int offset, CancellationToken ct);

    }
}
