namespace GoPost.Models.ViewModels
{
    public class HomePageViewModel
    {
        public List<UserProfileViewModel> SuggestedUsers { get; set; }
        public List<Post> FollowingPosts { get; set; }
        public List<Post> CurrentUserPosts { get; set; }
    }
}
