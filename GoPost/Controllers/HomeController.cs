using GoPost.Data;
using GoPost.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GoPost.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var suggestedUsers = await _context.Users
            .Where(u => u.Id != currentUserId &&
                    !_context.Follows.Any(f => f.FollowerId == currentUserId && f.FolloweeId == u.Id))
                .Take(5)
                .Select(u => new UserSuggestionViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    IsFollowed = false // optional, if you want to support toggling later
                })
                .ToListAsync();

            return View(suggestedUsers);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}