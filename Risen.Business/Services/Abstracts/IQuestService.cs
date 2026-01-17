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
        Task<QuestDto> GetQuestAsync(Guid questId, CancellationToken ct);
        Task<SubmitQuestAnswerResponse> SubmitAnswerAsync(Guid questId, Guid userId, int selectedIndex, CancellationToken ct);
    }
}
