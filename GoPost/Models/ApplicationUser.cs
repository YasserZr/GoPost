using GoPost.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

public class ApplicationUser : IdentityUser
{
    public ICollection<Post> Posts { get; set; }
    public ICollection<Reaction> Reactions { get; set; }
    public ICollection<Follow> Followers { get; set; }
    public ICollection<Follow> Following { get; set; }
    public ICollection<Comment> Comments { get; set; } // 🔥 Optional
}