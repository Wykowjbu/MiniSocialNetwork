using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Users
{
    public class ProfileModel : PageModel
    {
        private readonly IProfileService _profileService;
        private readonly IPostService _postService;
        private readonly IFollowService _followService;

        public ProfileModel(IProfileService profileService, IPostService postService, IFollowService followService)
        {
            _profileService = profileService;
            _postService = postService;
            _followService = followService;
        }

        public ProfileViewModel? Profile { get; set; }
        public IEnumerable<PostViewModel>? UserPosts { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool IsOwnProfile { get; set; }
        public bool IsFollowing { get; set; }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            Profile = await _profileService.GetProfileByUsernameAsync(username);
            if (Profile == null)
            {
                return NotFound();
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            IsLoggedIn = currentUserId != null;
            IsOwnProfile = currentUserId.HasValue && currentUserId.Value == Profile.UserId;

            if (IsLoggedIn && !IsOwnProfile)
            {
                IsFollowing = await _followService.IsFollowingAsync(currentUserId!.Value, Profile.UserId);
            }

            UserPosts = await _postService.GetUserPostsAsync(Profile.UserId, currentUserId);

            return Page();
        }

        public async Task<IActionResult> OnPostToggleFollowAsync(int targetUserId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _followService.ToggleFollowAsync(userId.Value, targetUserId);

            // Get username to redirect back
            var profile = await _profileService.GetProfileAsync(targetUserId);
            return RedirectToPage("/Users/Profile", new { username = profile?.Username });
        }

        public async Task<IActionResult> OnPostLikeAsync(int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _postService.ToggleLikeAsync(postId, userId.Value);

            // Get post to find the user
            var post = await _postService.GetPostAsync(postId, userId);
            return RedirectToPage("/Users/Profile", new { username = post?.Username });
        }
    }
}
