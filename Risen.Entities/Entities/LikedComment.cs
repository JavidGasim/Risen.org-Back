using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risen.Entities.Entities
{
    public class LikedComment
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public CustomIdentityUser? User { get; set; }
        public int CommentId { get; set; }
        public Comment? Comment { get; set; }
    }
}
