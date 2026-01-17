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
        public Guid QuestId { get; set; }
        public Guid UserId { get; set; }

        public int SelectedOptionIndex { get; set; } // 0..4
        public bool IsCorrect { get; set; }

        public int EarnedXp { get; set; }

        // === Köhnə property (servislər buna baxır) ===
        public DateTime? CompletedDateUtc { get; set; }
        public DateTime AnsweredAtUtc { get; set; } = DateTime.UtcNow;

    }
}
