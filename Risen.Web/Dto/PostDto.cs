namespace Risen.Web.Dto
{
    public class PostDto
    {
        public int Id { get; set; }

        public string Text { get; set; } = null!;

        public DateTime ShareDate { get; set; }

        public string SenderId { get; set; } = null!;

        public UserDto Sender { get; set; } = null!;

        public int LikeCount { get; set; }

        public int CommentCount { get; set; }

        public List<CommentDto> Comments { get; set; } = new();
    }
}
