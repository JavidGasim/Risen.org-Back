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

        public Guid SelectedOptionId { get; set; }
        public QuestOption SelectedOption { get; set; } = default!;

        public bool IsCorrect { get; set; }

        public int AwardedXp { get; set; } // bu submit-də verilən XP (0 ola bilər)

        public DateTime CompletedAtUtc { get; set; } // submit time

        public DateTime? CompletedDateUtc { get; set; } // yalnız “completion” sayılacaqsa set edilir
    }
}
