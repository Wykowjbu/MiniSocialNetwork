using System.ComponentModel.DataAnnotations;

namespace MiniSocialNetwork.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 1000 characters")]
        public string Content { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        // User info
        public string Username { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }

        public bool IsOwnedByCurrentUser { get; set; }
    }
}
