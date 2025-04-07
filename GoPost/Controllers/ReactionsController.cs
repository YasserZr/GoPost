using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GoPost.Models;
using GoPost.Data;
using Microsoft.EntityFrameworkCore;

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
        return RedirectToAction("Details", "Posts", new { id = postId });
    }

}
