namespace GoPost.Models.ViewModels
{
    public class HomePageViewModel
    {
        public List<UserSuggestionViewModel> SuggestedUsers { get; set; }
        public List<PostViewModel> FollowingPosts { get; set; }
        public List<PostViewModel> CurrentUserPosts { get; set; }
    }
}
