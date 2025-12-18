using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IQuestEntitlementService
    {
        Task<(bool IsPremium, string Plan, int DailyLimit, bool AdvancedAllowed)>
               GetQuestPolicyAsync(Guid userId, CancellationToken ct);
    }
}
