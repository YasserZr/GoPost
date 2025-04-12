namespace GoPost.Models.ViewModels
{
    public class PostViewModel
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public int LikeCount { get; set; }  // Add this line
        public int CommentCount { get; set; } // Add this line
        // Add other post properties you want to display
    }
}