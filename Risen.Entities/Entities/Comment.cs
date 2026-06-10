using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public DateTime? WritingDate { get; set; }
        public string? Content { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public string? SenderId { get; set; }
        public int LikeCount { get; set; } = 0;
        public CustomIdentityUser? Sender { get; set; }
    }
}
