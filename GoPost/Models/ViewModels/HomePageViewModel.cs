namespace GoPost.Models.ViewModels
{
    public class HomePageViewModel
    {
        public List<ApplicationUser> SuggestedUsers { get; set; }
        public List<Post> FollowingPosts { get; set; }
        public List<Post> CurrentUserPosts { get; set; }
    }
}
