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
        // actorId: the caller performing the award (admin or the user themself). Target user is read from req.TargetUserId or defaults to actorId.
        // commit: when true, the service will SaveChanges. When false the caller is expected to persist changes.
        Task<AwardXpResponse> AwardAsync(Guid actorId, AwardXpRequest req, CancellationToken ct, bool commit = true);
        Task<AwardXpResponse> RevokeAsync(Guid userId, Risen.Contracts.Gamification.RevokeXpRequest req, CancellationToken ct);
    }
}
