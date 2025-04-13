using GoPost.Data;
using GoPost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoPost.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CreateNotification(string senderId, string receiverId, string type, string content)
        {
            var notification = new Notification
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Type = type,
                Content = content,
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ReceiverId == userId)
                .Include(n => n.Sender) // Include sender details
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();

            return Json(notifications);
        }

        // Action to render the Notifications view
        [HttpGet]
        public IActionResult ViewNotifications()
        {
            return View("_Notifications"); // Returns the _Notifications.cshtml partial view
        }
    }
}
