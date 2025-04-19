namespace GoPost.Models.ViewModels
{
    public class UserProfileViewModel : ApplicationUser
    {
        public bool IsFollowed { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
       // public string Bio { get; set; } // Optional
        //public string ProfileImageUrl { get; set; } // Optional

        public List<Post> Posts { get; set; }

        // NEW: For Admin Panel (Lock/Unlock)
        public bool IsLocked { get; set; }
        
        // NEW: User Roles
        public List<string> Roles { get; set; }
    }
}
