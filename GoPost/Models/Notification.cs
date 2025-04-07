using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GoPost.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string SenderId { get; set; }

        [ForeignKey("SenderId")]
        public ApplicationUser Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public ApplicationUser Receiver { get; set; }

        [Required]
        public string Type { get; set; } // "onReaction", "onComment", "onFollow"

        public string? Content { get; set; } // Optional message (e.g. "Yasser liked your post")

        public bool IsRead { get; set; } = false;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
