namespace GoPost.Models
{
    public class Reaction
    {
        public int Id { get; set; }
        public string Type { get; set; } // e.g. "like", "love", etc.

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
