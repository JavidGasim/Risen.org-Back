using Risen.Contracts.Subjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface ISubjectService
    {
        Task<IReadOnlyList<SubjectDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    }
}
