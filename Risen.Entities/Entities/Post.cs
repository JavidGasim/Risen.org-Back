using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public DateTime ShareDate { get; set; }
        public string? SenderId { get; set; }
        public CustomIdentityUser? Sender { get; set; }
        public string? Text { get; set; }
        public int? LikeCount { get; set; } = 0;
        public int? CommentCount { get; set; } = 0;
        public List<Comment>? Comments { get; set; }
    }
}
