using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class XpTransaction
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public CustomIdentityUser User { get; set; } = default!;

        public XpSourceType SourceType { get; set; }

        // eyni quest üçün ikinci dəfə XP yazılmasın deyə idempotency key:
        public string SourceKey { get; set; } = default!; // məsələn: "Quest:9f...-attempt:1"

        public int BaseXp { get; set; }
        public decimal DifficultyMultiplier { get; set; } = 1.0m;
        public int FinalXp { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // If this transaction was created by an admin (e.g. revoke/adjust), store admin audit info
        public Guid? AdminId { get; set; }
        public string? AdminReason { get; set; }
    }
}
