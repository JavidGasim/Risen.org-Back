using Risen.Contracts.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IQuestQueryService
    {
        Task<TodayQuestsResponse> GetTodayAsync(Guid userId, CancellationToken ct);
        Task<QuestListItemDto?> GetByIdAsync(Guid userId, Guid questId, CancellationToken ct);
        Task<IReadOnlyList<QuestListItemDto>> GetCatalogAsync(Guid userId, CancellationToken ct);
    }
}
