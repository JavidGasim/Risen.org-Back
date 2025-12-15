using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class Plan
    {
        public Guid Id { get; set; }
        public PlanCode Code { get; set; }
        public string Name { get; set; } = default!;
    }
}
