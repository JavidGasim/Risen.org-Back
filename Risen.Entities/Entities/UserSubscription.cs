using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class UserSubscription
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }

        public DateTime StartsAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? EndsAtUtc { get; set; }     // null => müddətsiz (Lifetime) və ya “aktiv, tarixsiz”
        public bool IsActive { get; set; } = true;

        public Plan Plan { get; set; } = default!;
    }
}
