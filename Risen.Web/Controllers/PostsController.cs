using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Entities.Entities;
using Risen.Web.Hubs;
using Risen.Web.Mappers;
using System.ComponentModel.Design;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Risen.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly ILikedPostService _likedPostService;
        private readonly ILikedCommentService _likedCommentService;
        private readonly ILogger<PostsController> _logger;
        private readonly IHubContext<CommunityHub> _communityHub;

        public PostsController(UserManager<CustomIdentityUser> userManager, IPostService postService, ICommentService commentService, ILikedPostService likedPostService, ILikedCommentService likedCommentService, ILogger<PostsController> logger, IHubContext<CommunityHub> communityHub)
        {
            _userManager = userManager;
            _postService = postService;
            _commentService = commentService;
            _likedPostService = likedPostService;
            _likedCommentService = likedCommentService;
            _logger = logger;
            _communityHub = communityHub;
        }

        [Authorize]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var allPosts = await _postService.GetAllAsync();
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var likedPosts = await _likedPostService.GetAllAsync();
            var likedComments = await _likedCommentService.GetAllAsync();

            return Ok(allPosts.Select(x => x.ToDto()));
        }

        [Authorize]
        [HttpGet("getMyPosts")]
        public async Task<IActionResult> GetMyPosts()
        {
            var allPosts = await _postService.GetAllAsync();
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var likedPosts = await _likedPostService.GetByUserIdAsync(current.Id.ToString());
            var likedComments = await _likedCommentService.GetByUserIdAsync(current.Id.ToString());
            var myPosts = allPosts.Where(p => p.SenderId == current.Id.ToString());


            return Ok(new
            {
                posts = myPosts.Select(x => x.ToDto()),
                currentId = current.Id,
                likedPosts,
                likedComments
            });
        }

        [Authorize]
        [HttpPost("addPost")]
        public async Task<IActionResult> SharePost(string text)
        {
            var sender = await _userManager.GetUserAsync(HttpContext.User);

            var post = new Post
            {
                Text = text,
                SenderId = sender.Id.ToString(),
                Sender = sender,
                ShareDate = DateTime.Now,
                LikeCount = 0,
                CommentCount = 0,
                Comments = new List<Comment>()
            };

            await _postService.AddAsync(post);

            await _communityHub.Clients.All.SendAsync(
                "PostAdded",
                post.ToDto());

            return Ok(new { Message = "Post shared successfully" });
        }

        [Authorize]
        [HttpPost("deletePost")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _postService.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (post == null)
            {
                return NotFound(new { Message = "Post not found" });
            }

            await _postService.DeleteAsync(post);

            await _communityHub.Clients.All.SendAsync("PostDeleted", new
            {
                PostId = id
            });

            return Ok(new { Message = "Post deleted successfully" });
        }

        [Authorize]
        [HttpPost("addComment")]
        public async Task<IActionResult> AddComment(int id, string message)
        {
            var post = await _postService.GetByIdAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var comment = new Comment
            {
                PostId = post.Id,
                Content = message,
                WritingDate = DateTime.Now,
                SenderId = user.Id.ToString(),
                Sender = user
            };

            post.CommentCount++;

            await _commentService.AddAsync(comment);
            await _postService.UpdateAsync(post);

            await _communityHub.Clients.All.SendAsync("CommentAdded", post.Id);

            return Ok(new { Message = "Comment added successfully" });
        }

        [Authorize]
        [HttpPost("deleteComment")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound(new { Message = "Comment not found" });
            }

            var post = await _postService.GetByIdAsync(comment.PostId);

            if (post == null)
            {
                return NotFound(new { Message = " Post not found" });
            }

            post.CommentCount--;

            await _postService.UpdateAsync(post);
            await _commentService.DeleteAsync(comment);

            await _communityHub.Clients.All.SendAsync("CommentDeleted", post.Id);

            return Ok(new { Message = "Comment deleted successfully" });
        }

        [Authorize]
        [HttpPost("likePost")]
        public async Task<IActionResult> SendLike(int id, string currentId)
        {
            var post = await _postService.GetByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var likedPosts = await _likedPostService.GetAllAsync();
            var message = "";

            if (post == null)
            {
                return NotFound(new { Message = "Post not found" });
            }

            var likedPost = likedPosts.FirstOrDefault(l => l.UserId == currentUser.Id.ToString() && l.PostId == post.Id);

            if (likedPost == null)
            {
                message = "liked";
                post.LikeCount += 1;
                await _postService.UpdateAsync(post);

                var newLikedPost = new LikedPost()
                {
                    PostId = post.Id,
                    Post = post,
                    UserId = currentUser.Id.ToString(),
                    User = currentUser
                };

                await _likedPostService.AddAsync(newLikedPost);
            }
            else
            {
                message = "disliked";
                post.LikeCount -= 1;
                await _postService.UpdateAsync(post);
                await _likedPostService.DeleteAsync(likedPost);
            }



            await _communityHub.Clients.All.SendAsync("PostLikeChanged", new
            {
                PostId = post.Id,
                LikeCount = post.LikeCount ?? 0,
                UserId = currentUser.Id.ToString(),
                IsLiked = likedPost == null
            });

            return Ok(new { Message = $"Post {post.Id} liked//disliked successfully" });
        }

        [Authorize]
        [HttpPost("likeComment")]
        public async Task<IActionResult> SendCommentLike(int id, string senderId)
        {
            var comment = await _commentService.GetByIdAsync(id);
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var likedComments = await _likedCommentService.GetAllAsync();
            string message = "";

            if (comment == null)
            {
                return NotFound(new { Message = "Comment not found" });
            }


            var likedComment = likedComments.FirstOrDefault(l => l.UserId == currentUser.Id.ToString() && l.CommentId == comment.Id);

            if (likedComment == null)
            {
                message = "liked";

                comment.LikeCount += 1;
                await _commentService.UpdateAsync(comment);

                var newLikedComment = new LikedComment()
                {
                    CommentId = comment.Id,
                    Comment = comment,
                    UserId = currentUser.Id.ToString(),
                    User = currentUser
                };

                await _likedCommentService.AddAsync(newLikedComment);
            }
            else
            {
                message = "disliked";

                comment.LikeCount -= 1;
                await _commentService.UpdateAsync(comment);
                await _likedCommentService.DeleteAsync(likedComment);
            }




            await _communityHub.Clients.All.SendAsync("CommentLikeChanged", comment.Id);

            return Ok(new { Message = $"Comment {comment.Id} liked//disliked successfully" });
        }
    }
}