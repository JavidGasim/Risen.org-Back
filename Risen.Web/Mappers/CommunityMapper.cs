using Risen.Entities.Entities;
using Risen.Web.Dto;

namespace Risen.Web.Mappers
{
    public static class CommunityMapper
    {
        public static PostDto ToDto(this Post post)
        {
            return new PostDto
            {
                Id = post.Id,
                Text = post.Text,
                ShareDate = post.ShareDate,
                SenderId = post.SenderId,
                LikeCount = post.LikeCount ?? 0,
                CommentCount = post.CommentCount ?? 0,

                Sender = new UserDto
                {
                    Id = post.Sender.Id.ToString(),
                    FullName = post.Sender.FullName,
                    UserName = post.Sender.UserName
                },

                Comments = post.Comments
                    .Select(c => c.ToDto())
                    .ToList()
            };
        }

        public static CommentDto ToDto(this Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                WritingDate = comment.WritingDate ?? DateTime.Now,
                PostId = comment.PostId,
                LikeCount = comment.LikeCount,
                SenderId = comment.SenderId,

                Sender = new UserDto
                {
                    Id = comment.Sender.Id.ToString(),
                    FullName = comment.Sender.FullName,
                    UserName = comment.Sender.UserName
                }
            };
        }
    }
}
