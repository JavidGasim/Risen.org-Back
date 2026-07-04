using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.Entities.Entities;

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

        public PostsController(UserManager<CustomIdentityUser> userManager, IPostService postService, ICommentService commentService, ILikedPostService likedPostService, ILikedCommentService likedCommentService, ILogger<PostsController> logger)
        {
            _userManager = userManager;
            _postService = postService;
            _commentService = commentService;
            _likedPostService = likedPostService;
            _likedCommentService = likedCommentService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var allPosts = await _postService.GetAllAsync();
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var likedPosts = await _likedPostService.GetAllAsync();
            var likedComments = await _likedCommentService.GetAllAsync();

            return Ok(allPosts);
        }

        [Authorize]
        [HttpGet("getMyPosts")]
        public async Task<IActionResult> GetMyPosts()
        {
            var allPosts = await _postService.GetAllAsync();
            var current = await _userManager.GetUserAsync(HttpContext.User);
            var likedPosts = await _likedPostService.GetAllAsync();
            var myPosts = allPosts.Where(p => p.SenderId == current.Id.ToString());
            var likedComments = await _likedCommentService.GetAllAsync();


            return Ok(new { posts = myPosts, currentId = current.Id, likedPosts = likedPosts, likedComments = likedComments });
        }

        [Authorize]
        [HttpPost("addPost")]
        public async Task<IActionResult> SharePost(string text)
        {
            var sender = await _userManager.GetUserAsync(HttpContext.User);

            await _postService.AddAsync(new Post
            {
                Text = text,
                SenderId = sender.Id.ToString(),
                Sender = sender,
                ShareDate = DateTime.Now
            });

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

            return Ok(new { Message = "Comment added successfully" });
        }

        [Authorize]
        [HttpPost("deleteComment")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var post = await _postService.GetByIdAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound(new { Message = "Comment not found" });
            }

            post.CommentCount--;

            await _postService.UpdateAsync(post);
            await _commentService.DeleteAsync(comment);

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
            if (post != null)
            {
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

            }
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
            if (comment != null)
            {
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


            }

            return Ok(new { Message = $"Comment {comment.Id} liked//disliked successfully" });
        }
    }
}