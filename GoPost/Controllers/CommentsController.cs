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

            var comment = new Comment
            {
                PostId = postId,
                Content = content.Trim(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

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
    }
}
