using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class EntitlementService : IEntitlementService
    {
        private readonly AppDbContext _db;

        public EntitlementService(AppDbContext db) => _db = db;

        public async Task<(bool IsPremium, string Plan)> GetUserEntitlementAsync(Guid userId, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var sub = await _db.UserSubscriptions
                .Include(x => x.Plan)
                .Where(x => x.UserId == userId && x.IsActive)
                .OrderByDescending(x => x.StartsAtUtc)
                .FirstOrDefaultAsync(ct);

            if (sub is null)
                return (false, PlanCode.Free.ToString());

            var stillValid = !sub.EndsAtUtc.HasValue || sub.EndsAtUtc.Value > now;
            if (!stillValid)
                return (false, PlanCode.Free.ToString());

            var plan = sub.Plan.Code;
            var premium = plan is PlanCode.Premium or PlanCode.Lifetime;

            return (premium, plan.ToString());
        }
    }
}
