using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GoPost.Models;
using GoPost.Data;

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
    public async Task<IActionResult> Create(int PostId, string Content)
    {
        var user = await _userManager.GetUserAsync(User);

        var comment = new Comment
        {
            PostId = PostId,
            Content = Content,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Post", new { id = PostId });
    }
}
