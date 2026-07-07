namespace Risen.Web.Dto
{
    public class CommentDto
    {
        public int Id { get; set; }

        public string Content { get; set; } = null!;

        public DateTime WritingDate { get; set; }

        public int PostId { get; set; }

        public int LikeCount { get; set; }

        public string SenderId { get; set; } = null!;

        public UserDto Sender { get; set; } = null!;
    }
}
