using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Contracts.Subjects;
using Risen.DataAccess.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class SubjectService : ISubjectService
    {
        private readonly AppDbContext _db;
        public SubjectService(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<SubjectDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
        {
            var q = _db.Subjects.AsNoTracking().AsQueryable();
            if (!includeInactive) q = q.Where(s => s.IsActive);

            var items = await q.OrderBy(s => s.Name).ToListAsync(ct);
            return items.Select(s => new SubjectDto(s.Code, s.Name, s.Description, s.IsActive)).ToList();
        }
    }
}
