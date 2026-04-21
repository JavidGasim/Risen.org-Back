using System;
using System.Threading;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IAdminAuditService
    {
        Task RecordAsync(Guid adminId, string actionType, string details, Guid? targetUserId = null, CancellationToken ct = default);
    }
}
