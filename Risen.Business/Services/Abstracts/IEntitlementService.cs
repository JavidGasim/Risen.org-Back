using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IEntitlementService
    {
        Task<(bool IsPremium, string Plan)> GetUserEntitlementAsync(Guid userId, CancellationToken ct);
    }
}
