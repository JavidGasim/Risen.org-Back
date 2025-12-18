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
        Task<ClaimXpResponse> ClaimAsync(Guid userId, ClaimXpRequest req, CancellationToken ct);
    }
}
