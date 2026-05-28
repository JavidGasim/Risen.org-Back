using System;

namespace Risen.Entities.Entities
{
    public class PlanEntitlement
    {
        public Guid Id { get; set; }

        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = default!;

        // Entitlement key (e.g., "DailyQuestLimit", "AdvancedQuestsAllowed", "PremiumMultiplier")
        public string EntitlementKey { get; set; } = default!;

        // Entitlement value (e.g., "100", "true", "1.5")
        public string EntitlementValue { get; set; } = default!;

        // Description for admin
        public string? Description { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
