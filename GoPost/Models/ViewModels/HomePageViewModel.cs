namespace GoPost.Models.ViewModels
{
    public class HomePageViewModel
    {
        public List<UserSuggestionViewModel> SuggestedUsers { get; set; }
        public List<Post> FollowingPosts { get; set; }
        public List<Post> CurrentUserPosts { get; set; }
    }
}
