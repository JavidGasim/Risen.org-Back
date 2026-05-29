using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Risen.Business.Options;
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
    public class QuestEntitlementService : IQuestEntitlementService
    {
        private readonly AppDbContext _db;
        private readonly IEntitlementService _entitlementService;

        public QuestEntitlementService(AppDbContext db, IEntitlementService entitlementService)
        {
            _db = db;
            _entitlementService = entitlementService;
        }

        public async Task<(bool IsPremium, string Plan, int DailyLimit, bool AdvancedAllowed)>
            GetQuestPolicyAsync(Guid userId, CancellationToken ct)
        {
            var (isPremium, plan) = await _entitlementService.GetUserEntitlementAsync(userId, ct);

            // Get plan from database to get current settings
            var planEntity = await _db.Plans.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == plan, ct);

            var dailyLimit = planEntity?.DailyQuestLimit ?? 10;  // Default to Free limit
            var advancedAllowed = planEntity?.AllowAdvancedQuests ?? false;

            return (isPremium, plan.ToString(), dailyLimit, advancedAllowed);
        }
    }
}
