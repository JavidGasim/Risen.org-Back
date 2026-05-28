namespace Risen.Entities.Entities
{
    public class Plan
    {
        public Guid Id { get; set; }
        public PlanCode Code { get; set; }
        public string Name { get; set; } = default!;

        // Plan Features
        public int DailyQuestLimit { get; set; } = 10;           // Default: Free = 10
        public bool AllowAdvancedQuests { get; set; } = false;    // Default: False
        public decimal XpMultiplier { get; set; } = 1.0m;         // Default: 1.0x
        public string? Description { get; set; }                  // Admin notları

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
