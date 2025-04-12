using GoPost.Data;
using GoPost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GoPost.Models.ViewModels;

namespace GoPost.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Fetch user by id and include related posts
        public async Task<IActionResult> Index(string id)
        {
            var user = await _context.Users
                .OfType<ApplicationUser>() // Ensure the query is against ApplicationUser
                .Include(u => u.Posts) // Including related posts
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // You can pass the user and posts to the view
            return View(user);
        }

        public async Task<IActionResult> Profile(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var user = await _context.Users
                .OfType<ApplicationUser>()
                .Include(u => u.Posts) // Including related posts
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            var isFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FolloweeId == userId);

            var profileViewModel = new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl ?? "/images/default-profile.png",  // Fallback to default image if null
                Posts = user.Posts?.ToList() ?? new List<Post>(),  // Use empty list if Posts is null
                FollowersCount = user.Followers?.Count ?? 0,  // Use 0 if Followers is null
                FollowingCount = user.Following?.Count ?? 0,  // Use 0 if Following is null
                IsFollowed = isFollowing
            };


            ViewData["IsOwnProfile"] = currentUserId == userId;
            ViewData["IsFollowing"] = isFollowing;

            return View("UserProfile", profileViewModel); // Make sure "UserProfile" is the name of your view
        }



        [HttpPost]
        public async Task<IActionResult> UpdateProfileImage(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);

            if (file != null && file.Length > 0)
            {
                // You can implement your file saving logic here. For example:
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                user.ProfileImageUrl = $"/images/{file.FileName}";  // Save image URL
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("Profile", new { userId = user.Id });
        }

    }
}
