using Risen.Contracts.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IQuestService
    {
        Task<CompleteQuestResponse> CompleteAsync(Guid userId, CompleteQuestRequest req, CancellationToken ct);
    }
}
