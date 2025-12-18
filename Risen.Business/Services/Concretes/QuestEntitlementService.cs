using Microsoft.Extensions.Options;
using Risen.Business.Options;
using Risen.Business.Services.Abstracts;
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
        private readonly IEntitlementService _entitlementService;
        private readonly QuestPolicyOptions _opt;

        public QuestEntitlementService(IEntitlementService entitlementService, IOptions<QuestPolicyOptions> opt)
        {
            _entitlementService = entitlementService;
            _opt = opt.Value;
        }

        public async Task<(bool IsPremium, string Plan, int DailyLimit, bool AdvancedAllowed)>
            GetQuestPolicyAsync(Guid userId, CancellationToken ct)
        {
            var (isPremium, plan) = await _entitlementService.GetUserEntitlementAsync(userId, ct);

            var dailyLimit = isPremium ? _opt.PremiumDailyQuestLimit : _opt.FreeDailyQuestLimit;
            var advancedAllowed = isPremium;

            return (isPremium, plan, dailyLimit, advancedAllowed);
        }
    }
}
