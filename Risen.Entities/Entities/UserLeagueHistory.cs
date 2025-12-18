using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class UserLeagueHistory
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public CustomIdentityUser User { get; set; } = default!;

        public Guid FromTierId { get; set; }
        public LeagueTier FromTier { get; set; } = default!;

        public Guid ToTierId { get; set; }
        public LeagueTier ToTier { get; set; } = default!;

        public long TotalXpAtChange { get; set; }
        public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
