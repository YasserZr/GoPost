using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GoPost.Data;
using GoPost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace GoPost.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: Posts
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var posts = await _context.Posts
                .Where(p => p.UserId == currentUserId)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }
        // GET: Posts Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }
        private bool PostExists(int id, string userId)
        {
            return _context.Posts.Any(e => e.Id == id && e.UserId == userId);
        }



        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User) // To show comment author
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }


        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Post post)
        {
            // Assign the UserId before model validation
            post.UserId = _userManager.GetUserId(User);

            Debug.WriteLine($"Assigned UserId: {post.UserId}");

            // Skip validation for UserId and User fields (since they are assigned manually)
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("Model state is invalid.");
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        Debug.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return View(post);
            }

            post.CreatedAt = DateTime.Now; // optional, if you want
            _context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }







        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                Debug.WriteLine("Edit: id is null.");
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                Debug.WriteLine($"Edit: Post with id {id} not found.");
                return NotFound();
            }

            // Ensure the post belongs to the current user
            var currentUserId = _userManager.GetUserId(User);
            Debug.WriteLine($"Edit: Current UserId: {currentUserId}, Post UserId: {post.UserId}");

            if (post.UserId != currentUserId)
            {
                Debug.WriteLine("Edit: Unauthorized access attempt.");
                return Unauthorized(); // Prevent unauthorized users from editing
            }

            return View(post);
        }
        // POST : Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,UserId")] Post post)
        {
            if (id != post.Id)
            {
                Debug.WriteLine($"Edit POST: id mismatch. Expected: {id}, Received: {post.Id}");
                return NotFound();
            }

            // Ensure the post belongs to the current user
            var currentUserId = _userManager.GetUserId(User);
            Debug.WriteLine($"Edit POST: Current UserId: {currentUserId}, Post UserId: {post.UserId}");

            if (post.UserId != currentUserId)
            {
                Debug.WriteLine("Edit POST: Unauthorized access attempt.");
                return Unauthorized(); // Prevent unauthorized access
            }

            // Explicitly remove User validation to prevent model state errors
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure UserId remains unchanged and set it explicitly
                    post.UserId = currentUserId;

                    Debug.WriteLine($"Edit POST: Saving Post. Id: {post.Id}, Content: {post.Content}, UserId: {post.UserId}");

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Edit POST: Post saved successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id, currentUserId))
                    {
                        Debug.WriteLine($"Edit POST: Post with id {post.Id} does not exist or doesn't belong to the user.");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                Debug.WriteLine("Edit POST: Model state is invalid.");
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        Debug.WriteLine($"Edit POST: Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            return View(post);
        }







        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId)
                return Unauthorized(); // Prevent deletion by other users

            return View(post);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId)
                return Unauthorized(); // Prevent deletion by other users

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }


}
