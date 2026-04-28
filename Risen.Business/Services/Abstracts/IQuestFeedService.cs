using Risen.Contracts.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IQuestFeedService
    {
        Task<TodayQuestsResponse> GetTodayAsync(Guid userId, int take, CancellationToken ct);
        Task<IReadOnlyList<Risen.Contracts.Quests.QuestListItemDto>> GetAllAsync(Guid userId, int limit, int offset, bool includeInactive, CancellationToken ct);

    }
}
