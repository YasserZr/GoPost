using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GoPost.Models;
using GoPost.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GoPost.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Comment cannot be empty.");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var post = await _context.Posts.FindAsync(postId); //get post

            var comment = new Comment
            {
                PostId = postId,
                Content = content.Trim(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Create notification for the post owner, if the commenter is not the owner
            if (post != null && post.UserId != user.Id)
            {
                var notificationsController = HttpContext.RequestServices.GetRequiredService<NotificationsController>(); //added
                await notificationsController.CreateNotification(user.Id, post.UserId, "onComment", $"{user.UserName} commented on your post: {content.Trim()}");

            }

            // Load the newly created comment with the user information
            var newComment = await _context.Comments
                .Where(c => c.Id == comment.Id)
                .Include(c => c.User)
                .Select(c => new // Project to a simpler object for JSON
                {
                    userName = c.User.UserName,
                    content = c.Content,
                    createdAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (newComment == null)
            {
                return StatusCode(500, "Failed to retrieve the new comment.");
            }

            return Json(newComment); // Return the new comment as JSON
        }

        [HttpGet]
        [AllowAnonymous] // Or require authorization if needed
        public async Task<IActionResult> GetCommentCount(int postId)
        {
            var commentCount = await _context.Comments
                .Where(c => c.PostId == postId)
                .CountAsync();

            return Json(new { commentCount = commentCount });
        }

        [HttpGet]
        [AllowAnonymous] // Or require authorization if needed
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    userName = c.User.UserName,
                    content = c.Content,
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            return Json(comments);
        }
    }
}
