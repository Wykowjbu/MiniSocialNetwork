using System.ComponentModel.DataAnnotations;

namespace MiniSocialNetwork.ViewModels
{
    public class FollowViewModel
    {
        public int FollowId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarPath { get; set; }
        public string? Bio { get; set; }
        public DateTime? FollowedAt { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
    }

    public class UserSearchViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarPath { get; set; }
        public string? Bio { get; set; }
        public int PostCount { get; set; }
        public int FollowerCount { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
    }
}
