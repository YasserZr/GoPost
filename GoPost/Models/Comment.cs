namespace GoPost.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Reference to the Post being commented on
        public int PostId { get; set; }
        public Post Post { get; set; }

        // 🔗 Reference to the user who wrote the comment
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

}
