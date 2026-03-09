using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MiniSocialNetwork.ViewModels
{
    public class PostViewModel
    {
        public int PostId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 5000 characters")]
        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public IFormFile? ImageFile { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // User info for display
        public string Username { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }

        // Statistics
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsOwnedByCurrentUser { get; set; }
    }
}
