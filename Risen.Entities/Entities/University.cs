using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class University
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string? StateProvince { get; set; }

        public string? PrimaryDomain { get; set; }
        public string? PrimaryWebPage { get; set; }

        // Dublikatları kontrol üçün (Name+Country+StateProvince-dən generate edəcəyik)
        public string NormalizedKey { get; set; } = default!;
    }
}
