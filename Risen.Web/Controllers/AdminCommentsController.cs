using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Risen.Business.Services.Abstracts;
using Risen.DataAccess.Data;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Controllers
{
    [Route("api/admin/comments")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCommentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdminCommentsController> _logger;
        private readonly IAdminAuditService _audit;

        public AdminCommentsController(AppDbContext db, ILogger<AdminCommentsController> logger, IAdminAuditService audit)
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

        // GET /api/admin/comments
        [HttpGet]
        public async Task<ActionResult> ListComments([FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 1000);
            offset = Math.Max(0, offset);

            var comments = await _db.Comments.AsNoTracking()
                .Include(c => c.Sender)
                .Include(c => c.Post)
                .OrderByDescending(c => c.WritingDate)
                .Skip(offset)
                .Take(limit)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.WritingDate,
                    SenderName = c.Sender!.FullName,
                    SenderEmail = c.Sender!.Email,
                    SenderId = c.SenderId,
                    c.LikeCount,
                    PostId = c.PostId,
                    PostPreview = c.Post!.Text!.Length > 50 ? c.Post.Text.Substring(0, 50) + "..." : c.Post.Text
                })
                .ToListAsync(ct);

            var total = await _db.Comments.CountAsync(ct);

            return Ok(new { limit, offset, items = comments, total });
        }

        // GET /api/admin/comments/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetComment(int id, CancellationToken ct = default)
        {
            var comment = await _db.Comments.AsNoTracking()
                .Include(c => c.Sender)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (comment is null) return NotFound("Comment not found.");

            return Ok(new
            {
                comment.Id,
                comment.Content,
                comment.WritingDate,
                SenderName = comment.Sender!.FullName,
                SenderEmail = comment.Sender!.Email,
                SenderId = comment.SenderId,
                comment.LikeCount,
                PostId = comment.PostId,
                PostText = comment.Post!.Text
            });
        }

        // PUT /api/admin/comments/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] AdminCommentUpdateRequest req, CancellationToken ct = default)
        {
            var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (comment is null) return NotFound("Comment not found.");

            var originalContent = comment.Content;
            comment.Content = req.Content;

            await _db.SaveChangesAsync(ct);

            var adminId = GetAdminId();
            try
            {
                await _audit.RecordAsync(adminId, "EditComment", $"Comment:{comment.Id}; PostId:{comment.PostId}; Old: {originalContent}; New: {req.Content}",
                    Guid.TryParse(comment.SenderId, out var uid) ? uid : null, ct);
            }
            catch { /* swallow audit errors */ }

            _logger.LogInformation("Admin {AdminId} edited comment {CommentId}", adminId, id);

            return NoContent();
        }

        // DELETE /api/admin/comments/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteComment(int id, CancellationToken ct = default)
        {
            var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (comment is null) return NotFound("Comment not found.");

            var postId = comment.PostId;
            var senderUserId = Guid.TryParse(comment.SenderId, out var uid) ? (Guid?)uid : null;

            // Decrement post comment count
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId, ct);
            if (post != null)
            {
                post.CommentCount = Math.Max(0, (post.CommentCount ?? 1) - 1);
                await _db.SaveChangesAsync(ct);
            }

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync(ct);

            var adminId = GetAdminId();
            try
            {
                await _audit.RecordAsync(adminId, "DeleteComment", $"Comment:{comment.Id}; PostId:{postId}; Content: {comment.Content}",
                    senderUserId, ct);
            }
            catch { /* swallow audit errors */ }

            _logger.LogInformation("Admin {AdminId} deleted comment {CommentId}", adminId, id);

            return NoContent();
        }
    }

    public sealed record AdminCommentUpdateRequest(
        string Content
    );
}
