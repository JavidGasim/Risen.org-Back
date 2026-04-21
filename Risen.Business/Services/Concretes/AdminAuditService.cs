using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class AdminAuditService : IAdminAuditService
    {
        private readonly AppDbContext _db;
        public AdminAuditService(AppDbContext db) => _db = db;

        public async Task RecordAsync(Guid adminId, string actionType, string details, Guid? targetUserId = null, CancellationToken ct = default)
        {
            var a = new AdminAction
            {
                Id = Guid.NewGuid(),
                AdminId = adminId,
                TargetUserId = targetUserId,
                ActionType = actionType,
                Details = details,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Add and SaveChanges but do not throw on failure to avoid breaking admin flows.
            _db.AdminActions.Add(a);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch
            {
                // swallow — auditing must not block admin operations
            }
        }
    }
}
