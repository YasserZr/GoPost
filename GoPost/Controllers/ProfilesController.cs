using GoPost.Data;
using GoPost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GoPost.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OfType<ApplicationUser>()
                .Include(u => u.Posts)
                .ToListAsync();

            var viewModels = users.Select(user => new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl ?? "/images/default-profile.png",
                Posts = user.Posts?.ToList() ?? new List<Post>(),
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow
            }).ToList();

            return View(viewModels); // Index.cshtml
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            var users = _context.Users
                .OfType<ApplicationUser>()
                .Select(u => new UserProfileViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    ProfileImageUrl = u.ProfileImageUrl,
                    IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTime.Now
                }).ToList();

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LockUser([FromBody] LockUnlockRequest model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId);
            if (user == null)
                return Json(new { success = false });

            user.LockoutEnd = DateTime.Now.AddYears(100); // Effectively locked
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UnlockUser([FromBody] LockUnlockRequest model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId);
            if (user == null)
                return Json(new { success = false });

            user.LockoutEnd = DateTime.Now; // Unlock
            _context.SaveChanges();
            return Json(new { success = true });
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

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<UserProfileViewModel>());
            }

            var users = await _context.Users
                .OfType<ApplicationUser>()
                .Where(u => u.UserName.Contains(query) || u.Email.Contains(query))
                .Include(u => u.Posts)
                .ToListAsync();

            var results = users.Select(user => new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl ?? "/images/default-profile.png",
                Posts = user.Posts?.ToList() ?? new List<Post>(),
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                IsFollowed = false // Optional, or calculate if needed
            }).ToList();

            return View("SearchResults", results); // You’ll need a view called SearchResults.cshtml
        }

    }
}
