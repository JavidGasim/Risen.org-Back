using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class QuestOption
    {
        public Guid Id { get; set; }
        public Guid QuestId { get; set; }
        public Quest Quest { get; set; } = default!;

        public int Index { get; set; } // 0..4
        public string Text { get; set; } = default!;
    }
}
