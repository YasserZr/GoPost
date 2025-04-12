namespace GoPost.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsFollowed { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public string Bio { get; set; } // Optional
        public string ProfileImageUrl { get; set; } // Optional

        public List<Post> Posts { get; set; }
    }
}
