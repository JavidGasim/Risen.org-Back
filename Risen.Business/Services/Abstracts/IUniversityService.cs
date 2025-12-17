using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Abstracts
{
    public interface IUniversityService
    {
        Task<Guid?> UpsertAndGetIdAsync(string? universityName, CancellationToken ct);

    }
}
