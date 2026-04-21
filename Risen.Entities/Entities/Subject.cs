using System;

namespace Risen.Entities.Entities
{
    public class Subject
    {
        public string Code { get; set; } = default!; // e.g., "algorithms"
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
