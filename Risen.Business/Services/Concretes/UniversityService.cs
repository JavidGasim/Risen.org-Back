using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Business.Services.Concretes
{
    public class UniversityService : IUniversityService
    {
        private readonly AppDbContext _db;
        public UniversityService(AppDbContext db) => _db = db;

        public async Task<Guid?> UpsertAndGetIdAsync(string? universityName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(universityName))
                return null;

            var name = universityName.Trim();
            var key = NormalizeKey(name);

            var existing = await _db.Universities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NormalizedKey == key, ct);

            if (existing is not null) return existing.Id;

            var uni = new University
            {
                Id = Guid.NewGuid(),
                Name = name,
                NormalizedKey = key,
                Country = null,            // <-- artıq problem deyil
                StateProvince = null,
                PrimaryDomain = null,
                PrimaryWebPage = null
            };

            _db.Universities.Add(uni);
            await _db.SaveChangesAsync(ct);

            return uni.Id;
        }

        private static string NormalizeKey(string name)
            => name.Trim().ToUpperInvariant();
    }
}
