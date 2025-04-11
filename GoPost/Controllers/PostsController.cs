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
        public async Task<IActionResult> Index(string searchString)
        {
            var currentUserId = _userManager.GetUserId(User);

            var postsQuery = _context.Posts
                .Where(p => p.UserId == currentUserId)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable(); // 👈 This enables further chaining

            if (!string.IsNullOrEmpty(searchString))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(searchString));
            }

            var posts = await postsQuery.ToListAsync(); // 👈 Now we execute the query
            return View(posts);
        }


        // GET: Posts Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllPosts(string searchString)
        {
            var postsQuery = _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable(); // 👈 Make sure it's IQueryable so we can chain

            if (!string.IsNullOrEmpty(searchString))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(searchString));
            }

            var posts = await postsQuery.ToListAsync(); // 👈 await here gives a List<Post>

            return View(posts);
        }

        public async Task<IActionResult> Search(string searchString)
        {
            var currentUserId = _userManager.GetUserId(User);

            IQueryable<Post> posts = _context.Posts
                .Where(p => p.UserId == currentUserId)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt);

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(p => p.Title.Contains(searchString));
            }

            var result = await posts.ToListAsync();
            return PartialView("_PostsTable", result);
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
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.PostFiles)  // Include files with the post
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId)
            {
                return Unauthorized();
            }

            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,UserId,ImagePath")] Post post, IFormFile imageFile, List<IFormFile> newFiles, List<int> fileDeleteIds)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId)
            {
                return Unauthorized();
            }

            // Get the existing post from the database
            var existingPost = await _context.Posts
                .Include(p => p.PostFiles)  // Get the related files
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPost == null)
            {
                return NotFound();
            }
            // Update the title of the post
            existingPost.Title = post.Title;

            // Update the content of the post
            existingPost.Content = post.Content;

            // Update the image if a new one is uploaded
            if (imageFile != null && imageFile.Length > 0)
            {
                // Delete the existing image if there is one
                if (!string.IsNullOrEmpty(existingPost.ImagePath))
                {
                    var oldImagePath = Path.Combine("wwwroot", existingPost.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save the new image
                var uploadsDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                var imagePath = Path.Combine(uploadsDirectory, Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName));
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                existingPost.ImagePath = "/uploads/" + Path.GetFileName(imagePath);
            }

            // Handle the deletion of files
            if (fileDeleteIds != null && fileDeleteIds.Any())
            {
                foreach (var fileId in fileDeleteIds)
                {
                    var fileToDelete = existingPost.PostFiles.FirstOrDefault(f => f.Id == fileId);
                    if (fileToDelete != null)
                    {
                        // Delete the file from the server
                        var filePath = Path.Combine("wwwroot", fileToDelete.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        // Remove the file entry from the database
                        _context.PostFiles.Remove(fileToDelete);
                    }
                }
            }

            // Handle the addition of new files
            if (newFiles != null && newFiles.Any())
            {
                var uploadsDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                foreach (var file in newFiles)
                {
                    if (file.Length > 0)
                    {
                        var filePath = Path.Combine(uploadsDirectory, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existingPost.PostFiles.Add(new PostFile
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = "/uploads/" + Path.GetFileName(filePath)
                        });
                    }
                }
            }

            try
            {
                // Save the changes
                _context.Update(existingPost);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(post.Id, currentUserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFile(int fileId)
        {
            var postFile = await _context.PostFiles.FindAsync(fileId);
            if (postFile == null)
            {
                return Json(new { success = false, message = "File not found." });
            }

            // Delete file from the server
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", postFile.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Remove file entry from the database
            _context.PostFiles.Remove(postFile);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private bool PostExists(int postId, string userId)
        {
            return _context.Posts.Any(p => p.Id == postId && p.UserId == userId);
        }


    }
}
