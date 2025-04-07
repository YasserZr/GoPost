using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GoPost.Models;
using GoPost.Data;

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

        // Check if the user already liked this post
        var alreadyLiked = _context.Reactions
            .Any(r => r.PostId == postId && r.UserId == user.Id && r.Type == "like");

        if (!alreadyLiked)
        {
            var reaction = new Reaction
            {
                PostId = postId,
                UserId = user.Id,
                Type = "like"
            };

            _context.Reactions.Add(reaction);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", "Post", new { id = postId });
    }
}
