using GoPost.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GoPost.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Post content is required.")]
        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Skip validation for UserId and User properties during creation
        [SkipValidationOnCreate] // Custom attribute to skip validation on these fields during creation
        public string UserId { get; set; }

        [SkipValidationOnCreate] // Custom attribute to skip validation on these fields during creation
        public ApplicationUser User { get; set; }

        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public bool IsLikedBy(string userId)
        {
            return Reactions?.Any(r => r.UserId == userId) ?? false;
        }
    }
}
