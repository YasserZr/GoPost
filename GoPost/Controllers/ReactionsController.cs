using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GoPost.Models;
using GoPost.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[Authorize]
public class ReactionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReactionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Like(int postId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var existingReaction = await _context.Reactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == user.Id && r.Type == "like");

        if (existingReaction != null)
        {
            // If already liked, remove the like
            _context.Reactions.Remove(existingReaction);
        }
        else
        {
            // Remove dislike if it exists
            var existingDislike = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == user.Id && r.Type == "dislike");

            if (existingDislike != null)
            {
                _context.Reactions.Remove(existingDislike);
            }

            // Add like
            var reaction = new Reaction
            {
                PostId = postId,
                UserId = user.Id,
                Type = "like"
            };

            _context.Reactions.Add(reaction);
        }

        await _context.SaveChangesAsync();

        // Get the updated like count
        var likeCount = await _context.Reactions
            .CountAsync(r => r.PostId == postId && r.Type == "like");

        // Return the like count as JSON to update the UI
        return Json(likeCount);
    }

    [HttpPost]
    public async Task<IActionResult> Dislike(int postId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var existingReaction = await _context.Reactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == user.Id && r.Type == "dislike");

        if (existingReaction != null)
        {
            // If already disliked, remove the dislike
            _context.Reactions.Remove(existingReaction);
        }
        else
        {
            // Remove like if it exists
            var existingLike = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == user.Id && r.Type == "like");

            if (existingLike != null)
            {
                _context.Reactions.Remove(existingLike);
            }

            // Add dislike
            var reaction = new Reaction
            {
                PostId = postId,
                UserId = user.Id,
                Type = "dislike"
            };

            _context.Reactions.Add(reaction);
        }

        await _context.SaveChangesAsync();

        // Get the updated dislike count
        var dislikeCount = await _context.Reactions
            .CountAsync(r => r.PostId == postId && r.Type == "dislike");

        // Return the dislike count as JSON to update the UI
        return Json(dislikeCount);
    }

    [HttpGet]
    [AllowAnonymous] // Or require authorization if needed
    public async Task<IActionResult> GetLikeCount(int postId)
    {
        var likeCount = await _context.Reactions
            .Where(r => r.PostId == postId && r.Type == "like")
            .CountAsync();

        return Json(new { likeCount = likeCount });
    }
}
