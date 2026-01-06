using Risen.Contracts.Gamification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IXpService
    {
        Task<AwardXpResponse> AwardAsync(Guid userId, AwardXpRequest req, CancellationToken ct);
    }
}
