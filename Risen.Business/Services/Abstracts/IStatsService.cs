using Risen.Contracts.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IStatsService
    {
        Task<MeStatsDto> GetMeAsync(Guid userId, CancellationToken ct);
    }
}
