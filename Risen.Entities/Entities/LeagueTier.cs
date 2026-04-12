using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class LeagueTier
    {
        public Guid Id { get; set; }
        public LeagueCode Code { get; set; }
        public string Name { get; set; } = default!;
        public long MinXp { get; set; }
        public long? MaxXp { get; set; }
        public int SortOrder { get; set; }
        public int Weight { get; set; }  // ← əlavə et
    }
}
