namespace GoPost.Models.ViewModels
{
    public class PostViewModel
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        // Add other post properties you want to display
    }
}