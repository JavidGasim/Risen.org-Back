using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class UserStats
    {
        public Guid UserId { get; set; }
        public CustomIdentityUser User { get; set; } = default!;

        public long TotalXp { get; set; }
        public Guid CurrentLeagueTierId { get; set; }
        public LeagueTier CurrentLeagueTier { get; set; } = default!;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastStreakDateUtc { get; set; }

        public decimal RisenScore { get; set; } = 0;  // ← əlavə et

    }
}
