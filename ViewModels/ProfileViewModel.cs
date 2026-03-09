using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MiniSocialNetwork.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string? Bio { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        public string? AvatarPath { get; set; }

        public IFormFile? AvatarFile { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int PostCount { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
    }
}
