using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class UserStats
    {
        public Guid UserId { get; set; }          // PK və FK
        public CustomIdentityUser User { get; set; } = default!;

        public long TotalXp { get; set; }         // all-time XP
        public Guid CurrentLeagueTierId { get; set; }
        public LeagueTier CurrentLeagueTier { get; set; } = default!;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastStreakDateUtc { get; set; } // yalnız date hissəsi (UtcNow.Date)

    }
}
