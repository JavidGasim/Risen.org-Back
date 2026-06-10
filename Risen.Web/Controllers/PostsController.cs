using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                ShareDate = DateTime.Now
            });

            return Ok();
        }
    }
}