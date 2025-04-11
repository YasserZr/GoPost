namespace GoPost.Models
{
    public class UserSuggestionViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsFollowed { get; set; }
        public string ProfileImageUrl { get; set; } // Optional if you have avatars
    }

}
