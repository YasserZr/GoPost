using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GoPost.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Post content is required.")]
        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional file and image paths
        public string? ImagePath { get; set; }

        public string? FilePath { get; set; }

        // UserId is required for every post. Set it manually in your controller.
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        // User navigation property to be loaded via the relationship.
        public ApplicationUser User { get; set; }  // It will be populated automatically by EF Core.

        // Navigation property
        public ICollection<PostFile> PostFiles { get; set; } = new List<PostFile>();

        // Optional collections for reactions and comments
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Helper method to check if post is liked by a user
        public bool IsLikedBy(string userId)
        {
            return Reactions?.Any(r => r.UserId == userId) ?? false;
        }
    }
}
