using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class QuestAttempt
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public CustomIdentityUser User { get; set; } = default!;

        public Guid QuestId { get; set; }
        public Quest Quest { get; set; } = default!;

        public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;

        // unikallıq üçün: “gün” ayrıca saxlanır
        public DateTime CompletedDateUtc { get; set; }  // DateTime.UtcNow.Date

        public int AwardedXp { get; set; }
    }
}
