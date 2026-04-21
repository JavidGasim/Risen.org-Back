using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class AdminAction
    {
        public Guid Id { get; set; }

        public Guid AdminId { get; set; }

        public Guid? TargetUserId { get; set; }

        public string ActionType { get; set; } = default!; // e.g., "AwardXp", "RevokeXp"

        public string Details { get; set; } = default!;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
