using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class Quest
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;

        public SubjectCode SubjectCode { get; set; } = SubjectCode.General; // <-- əlavə et

        public QuestDifficulty Difficulty { get; set; }
        public int BaseXp { get; set; }

        public bool IsPremiumOnly { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
