using GoPost.Data;
using GoPost.Models;
using GoPost.Models.ViewModels;
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

            // Get Suggested users
            var suggestedUsers = await _context.Users
                .Where(u => u.Id != currentUserId &&
                            !_context.Follows.Any(f => f.FollowerId == currentUserId && f.FolloweeId == u.Id))
                .Take(15)
                .Select(u => new UserProfileViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    IsFollowed = false // optional, if you want to support toggling later
                })
                .ToListAsync();

            // Get recent posts of followed users
            var followingPosts = await _context.Follows
                .Where(f => f.FollowerId == currentUserId)
                .SelectMany(f => _context.Posts
                    .Include(p => p.User) // Include the User for UserName.  Important for efficiency
                    .Where(p => p.UserId == f.FolloweeId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3))
                .OrderByDescending(p => p.CreatedAt)
                .Take(3) // Ensure only the top 3 across all followed users
                .ToListAsync();

            // Get current user's recent posts
            List<Post> currentUserPosts = new List<Post>();
            if (!string.IsNullOrEmpty(currentUserId))
            {
                currentUserPosts = await _context.Posts
                    .Include(p => p.User)  // Include User here as well
                    .Where(p => p.UserId == currentUserId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToListAsync();
            }


            var viewModel = new HomePageViewModel
            {
                SuggestedUsers = suggestedUsers,
                FollowingPosts = followingPosts,
                CurrentUserPosts = currentUserPosts
            };

            return View(viewModel);
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

        // New Action to get Following Posts
        public async Task<IActionResult> GetFollowingPosts()
        {
            var currentUserId = _userManager.GetUserId(User);

            var followingPosts = await _context.Follows
                .Where(f => f.FollowerId == currentUserId)
                .SelectMany(f => _context.Posts
                    .Include(p => p.User) // Include User
                    .Where(p => p.UserId == f.FolloweeId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3))
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();


            return Json(followingPosts);
        }
    }
}
