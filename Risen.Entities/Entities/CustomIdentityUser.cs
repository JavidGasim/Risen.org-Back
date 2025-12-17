using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class CustomIdentityUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;

        public string FullName { get; set; } = default!;
        public string? Country { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Guid? UniversityId { get; set; }
        public University? University { get; set; }
        public DateTime? LastOnlineAtUtc { get; set; }

    }
}
