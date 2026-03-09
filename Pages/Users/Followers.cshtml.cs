using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Users
{
    public class FollowersModel : PageModel
    {
        private readonly IProfileService _profileService;
        private readonly IFollowService _followService;

        public FollowersModel(IProfileService profileService, IFollowService followService)
        {
            _profileService = profileService;
            _followService = followService;
        }

        public string Username { get; set; } = string.Empty;
        public IEnumerable<FollowViewModel>? Followers { get; set; }
        public bool IsLoggedIn { get; set; }
        public int? CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            var profile = await _profileService.GetProfileByUsernameAsync(username);
            if (profile == null)
            {
                return NotFound();
            }

            Username = username;
            CurrentUserId = HttpContext.Session.GetInt32("UserId");
            IsLoggedIn = CurrentUserId != null;

            Followers = await _followService.GetFollowersAsync(profile.UserId, CurrentUserId);
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
            return RedirectToPage("/Users/Followers", new { username = Username });
        }
    }
}
