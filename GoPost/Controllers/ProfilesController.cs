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
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
                Id = user.Id,
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
        public async Task<IActionResult> Users(string searchString, string roleFilter, bool? isLockedFilter)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Start with all users except the current user
            var users = _context.Users
                .OfType<ApplicationUser>()
                .Where(u => u.Id != currentUserId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(roleFilter) && (roleFilter == "user" || roleFilter == "admin"))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                users = users.Where(u => usersInRole.Contains(u));
            }

            if (isLockedFilter.HasValue)
            {
                if (isLockedFilter.Value)
                {
                    users = users.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                }
                else
                {
                    users = users.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
                }
            }

            var viewModels = await users
                .Select(u => new UserProfileViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    ProfileImageUrl = u.ProfileImageUrl,
                    IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTime.Now,
                    Roles = _userManager.GetRolesAsync(u).Result.ToList()
                }).ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.IsLockedFilter = isLockedFilter;

            return View(viewModels);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangeRole([FromBody] ChangeRoleRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return Json(new { success = false, error = "User not found." });
            }

            if (model.Role != "user" && model.Role != "admin")
            {
                return Json(new { success = false, error = "Invalid role." });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, model.Role);

            if (result.Succeeded)
            {
                return Json(new { success = true, userId = user.Id, newRole = model.Role });
            }
            else
            {
                return Json(new { success = false, error = "Failed to update user role." });
            }
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
                Id = user.Id,
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
        [Authorize]
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
                Id = user.Id,
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
