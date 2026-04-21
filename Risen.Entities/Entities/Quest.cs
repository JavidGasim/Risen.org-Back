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

        // === Title: EF Core migration uyğunluğu üçün saxlanır. ===
        // Yeni kodda istifadə etməyin — QuestionText istifadə edin.
        [Obsolete("Use QuestionText. Title will be removed in a future migration.")]
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public QuestDifficulty Difficulty { get; set; } = QuestDifficulty.Beginner;
        public string? SubjectCode { get; set; }

        public bool IsPremiumOnly { get; set; } = false;
        public int BaseXp { get; set; } = 10;

        // === Kanonik sual mətni — bütün servislərdə bu istifadə olunur ===
        public string QuestionText { get; set; } = default!;

        // 0..4 (A..E), yalnız 1 düzgün cavab
        public int CorrectOptionIndex { get; set; }

        public ICollection<QuestOption> Options { get; set; } = new List<QuestOption>();

        // digər
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
