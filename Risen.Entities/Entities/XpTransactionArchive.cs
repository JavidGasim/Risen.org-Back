using System;

namespace Risen.Entities.Entities
{
    public class XpTransactionArchive
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public XpSourceType SourceType { get; set; }
        public string SourceKey { get; set; } = default!;
        public int BaseXp { get; set; }
        public decimal DifficultyMultiplier { get; set; }
        public int FinalXp { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Guid? AdminId { get; set; }
        public string? AdminReason { get; set; }

        public DateTime ArchivedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
