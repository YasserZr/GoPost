using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GoPost.Data;
using GoPost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace GoPost.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
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
                    .ThenInclude(c => c.User) // Include comment authors
                .Include(p => p.Reactions)
                .Include(p => p.PostFiles) // ✅ Include file attachments
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
        public async Task<IActionResult> Create(Post post, IFormFile imageFile, List<IFormFile> fileAttachments)
        {
            // Ensure that UserId is assigned
            post.UserId = _userManager.GetUserId(User);
            post.CreatedAt = DateTime.UtcNow;

            // Remove the user-specific fields from validation if needed
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                return View(post);
            }

            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var imagePath = Path.Combine(uploadsDirectory, Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName));
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                post.ImagePath = "/uploads/" + Path.GetFileName(imagePath);
            }

            // Handle multiple file uploads
            var savedFilePaths = new List<string>();
            if (fileAttachments != null && fileAttachments.Any())
            {
                foreach (var file in fileAttachments)
                {
                    if (file.Length > 0)
                    {
                        var filePath = Path.Combine(uploadsDirectory, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        savedFilePaths.Add("/uploads/" + Path.GetFileName(filePath));
                    }
                }

                // Save all file paths
                foreach (var path in savedFilePaths)
                {
                    post.PostFiles.Add(new PostFile
                    {
                        FileName = Path.GetFileName(path),
                        FilePath = path
                    });
                }

            }

            // Save the new post to the database
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

            var currentUserId = _userManager.GetUserId(User);
            Debug.WriteLine($"Edit: Current UserId: {currentUserId}, Post UserId: {post.UserId}");

            if (post.UserId != currentUserId)
            {
                Debug.WriteLine("Edit: Unauthorized access attempt.");
                return Unauthorized();
            }

            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,UserId")] Post post, IFormFile imageFile, IFormFile fileAttachment)
        {
            if (id != post.Id)
            {
                Debug.WriteLine($"Edit POST: id mismatch. Expected: {id}, Received: {post.Id}");
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId)
            {
                Debug.WriteLine("Edit POST: Unauthorized access attempt.");
                return Unauthorized();
            }

            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPost = await _context.Posts.FindAsync(id);
                    if (existingPost == null)
                    {
                        Debug.WriteLine("Edit POST: Post not found.");
                        return NotFound();
                    }

                    existingPost.Content = post.Content;

                    // Update image if a new one is uploaded
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imagePath = Path.Combine("wwwroot/uploads", Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName));
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        existingPost.ImagePath = "/uploads/" + Path.GetFileName(imagePath);
                        Debug.WriteLine($"Image updated: {existingPost.ImagePath}");
                    }

                    // Update file if a new one is uploaded
                    if (fileAttachment != null && fileAttachment.Length > 0)
                    {
                        var filePath = Path.Combine("wwwroot/uploads", Guid.NewGuid().ToString() + Path.GetExtension(fileAttachment.FileName));
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fileAttachment.CopyToAsync(stream);
                        }
                        existingPost.FilePath = "/uploads/" + Path.GetFileName(filePath);
                        Debug.WriteLine($"File updated: {existingPost.FilePath}");
                    }

                    _context.Update(existingPost);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Post edited successfully!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id, currentUserId))
                    {
                        Debug.WriteLine($"Edit POST: Post with id {post.Id} not found or doesn't belong to the user.");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            Debug.WriteLine("Edit POST: Model state is invalid.");
            return View(post);
        }

        // GET: Posts/Delete/5
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
                return Unauthorized();

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var post = await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            // Only allow the owner of the post to delete it
            if (post.UserId != currentUserId)
                return Unauthorized();

            // Remove associated comments
            if (post.Comments != null && post.Comments.Any())
            {
                _context.Comments.RemoveRange(post.Comments);
            }

            // Remove associated reactions
            if (post.Reactions != null && post.Reactions.Any())
            {
                _context.Reactions.RemoveRange(post.Reactions);
            }

            // Delete associated image file
            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Delete associated file attachment
            if (!string.IsNullOrEmpty(post.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Remove the post itself
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
