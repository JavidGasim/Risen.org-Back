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
        public string NormalizedKey { get; set; } = default!;

        public string? Country { get; set; }          // <-- nullable
        public string? StateProvince { get; set; }
        public string? PrimaryDomain { get; set; }
        public string? PrimaryWebPage { get; set; }
    }
}
