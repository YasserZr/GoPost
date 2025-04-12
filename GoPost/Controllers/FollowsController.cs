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

namespace GoPost.Controllers
{
    public class FollowsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> FollowUser(string userIdToFollow)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (userIdToFollow == currentUserId)
                return BadRequest("You cannot follow yourself.");

            bool alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FolloweeId == userIdToFollow);

            if (!alreadyFollowing)
            {
                var follow = new Follow
                {
                    FollowerId = currentUserId,
                    FolloweeId = userIdToFollow
                };
                _context.Follows.Add(follow);
                await _context.SaveChangesAsync();
            }

            return Ok(); // <-- No redirect, just a success
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFollow(string userIdToFollow)
        {
            var currentUserId = _userManager.GetUserId(User);
            ViewBag.CurrentUserId = currentUserId;


            if (userIdToFollow == currentUserId)
                return BadRequest("You cannot follow yourself.");

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FolloweeId == userIdToFollow);

            bool isFollowed;

            if (follow != null)
            {
                _context.Follows.Remove(follow);
                isFollowed = false;
            }
            else
            {
                _context.Follows.Add(new Follow
                {
                    FollowerId = currentUserId,
                    FolloweeId = userIdToFollow
                });
                isFollowed = true;
            }

            await _context.SaveChangesAsync();

            // You could also return follower count here if you want
            var followersCount = await _context.Follows.CountAsync(f => f.FolloweeId == userIdToFollow);

            return Json(new
            {
                isFollowed = isFollowed,
                followersCount = followersCount
            });

        }

        // New methods to get follower and following counts
        [HttpGet]
        public async Task<IActionResult> GetFollowerCount(string userId)
        {
            var followerCount = await _context.Follows.CountAsync(f => f.FolloweeId == userId);
            return Ok(followerCount);
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowingCount(string userId)
        {
            var followingCount = await _context.Follows.CountAsync(f => f.FollowerId == userId);
            return Ok(followingCount);
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowers(string userId)
        {
            var followers = await _context.Follows
                .Where(f => f.FolloweeId == userId)
                .Include(f => f.Follower) // Include the Follower (ApplicationUser)
                .Select(f => new
                {
                    UserId = f.FollowerId,
                    UserName = f.Follower.UserName // Access properties of the Follower
                })
                .ToListAsync();

            return Json(followers);
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowing(string userId)
        {
            var following = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Followee)  // Include the Followee
                .Select(f => new
                {
                    UserId = f.FolloweeId,
                    UserName = f.Followee.UserName
                })
                .ToListAsync();
            return Json(following);
        }
    }

    


}
