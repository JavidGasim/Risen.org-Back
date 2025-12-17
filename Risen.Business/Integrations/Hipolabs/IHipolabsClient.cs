using Risen.Business.Integrations.Hipolabs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Integrations.Hipolabs
{
    public interface IHipolabsClient
    {
        Task<List<HipolabsUniversityDto>> SearchAsync(string? name, string? country, int limit, int offset, CancellationToken ct);
    }
}
