using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/admin/posts")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminPostsController> _logger;
        private readonly IAdminAuditService _audit;

        public AdminPostsController(AppDbContext db, ILogger<AdminPostsController> logger, IAdminAuditService audit)
        {
            _db = db;
            _logger = logger;
            _audit = audit;
        }

        private Guid GetAdminId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(idStr, out var gid) ? gid : Guid.Empty;
        }

        // GET /api/admin/posts
        [HttpGet]
        public async Task<ActionResult> ListPosts([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var posts = await _db.Posts.AsNoTracking()
                .Include(p => p.Sender)
                .OrderByDescending(p => p.ShareDate)
                .Skip(offset)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Text,
                    p.ShareDate,
                    SenderName = p.Sender!.FullName,
                    SenderEmail = p.Sender!.Email,
                    SenderId = p.SenderId,
                    p.LikeCount,
                    p.CommentCount
                })
                .ToListAsync(ct);

            var total = await _db.Posts.CountAsync(ct);

            return Ok(new { limit, offset, items = posts, total });
        }

        // GET /api/admin/posts/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetPost(int id, CancellationToken ct = default)
        {
            var post = await _db.Posts.AsNoTracking()
                .Include(p => p.Sender)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (post is null) return NotFound("Post not found.");

            return Ok(new
            {
                post.Id,
                post.Text,
                post.ShareDate,
                SenderName = post.Sender!.FullName,
                SenderEmail = post.Sender!.Email,
                SenderId = post.SenderId,
                post.LikeCount,
                post.CommentCount,
                Comments = post.Comments?.Count ?? 0
            });
        }

        // PUT /api/admin/posts/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] AdminPostUpdateRequest req, CancellationToken ct = default)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (post is null) return NotFound("Post not found.");

            var originalText = post.Text;
            post.Text = req.Text;

            await _db.SaveChangesAsync(ct);

            var adminId = GetAdminId();
            try
            {
                await _audit.RecordAsync(adminId, "EditPost", $"Post:{post.Id}; Old: {originalText}; New: {req.Text}", 
                    Guid.TryParse(post.SenderId, out var uid) ? uid : null, ct);
            }
            catch { /* swallow audit errors */ }

            _logger.LogInformation("Admin {AdminId} edited post {PostId}", adminId, id);

            return NoContent();
        }

        // DELETE /api/admin/posts/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePost(int id, CancellationToken ct = default)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (post is null) return NotFound("Post not found.");

            var senderUserId = Guid.TryParse(post.SenderId, out var uid) ? (Guid?)uid : null;

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync(ct);

            var adminId = GetAdminId();
            try
            {
                await _audit.RecordAsync(adminId, "DeletePost", $"Post:{post.Id}; Text: {post.Text}", senderUserId, ct);
            }
            catch { /* swallow audit errors */ }

            _logger.LogInformation("Admin {AdminId} deleted post {PostId}", adminId, id);

            return NoContent();
        }
    }

    public sealed record AdminPostUpdateRequest(
        string Text
    );
}
