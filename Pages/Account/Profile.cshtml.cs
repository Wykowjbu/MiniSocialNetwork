using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly IProfileService _profileService;

        public ProfileModel(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [BindProperty]
        public ProfileViewModel Profile { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var profile = await _profileService.GetProfileAsync(userId.Value);
            if (profile == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Profile = profile;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Remove validation for file upload since it's optional
            ModelState.Remove("Profile.AvatarFile");

            if (!ModelState.IsValid)
            {
                // Reload profile stats
                var currentProfile = await _profileService.GetProfileAsync(userId.Value);
                if (currentProfile != null)
                {
                    Profile.PostCount = currentProfile.PostCount;
                    Profile.FollowerCount = currentProfile.FollowerCount;
                    Profile.FollowingCount = currentProfile.FollowingCount;
                }
                return Page();
            }

            Profile.UserId = userId.Value;

            // Handle avatar upload
            if (Profile.AvatarFile != null && Profile.AvatarFile.Length > 0)
            {
                var avatarPath = await _profileService.UploadAvatarAsync(userId.Value, Profile.AvatarFile);
                if (avatarPath != null)
                {
                    Profile.AvatarPath = avatarPath;
                    HttpContext.Session.SetString("AvatarPath", avatarPath);
                }
            }

            var success = await _profileService.UpdateProfileAsync(Profile);
            if (success)
            {
                // Update session
                HttpContext.Session.SetString("Username", Profile.Username);
                HttpContext.Session.SetString("Email", Profile.Email);

                SuccessMessage = "Profile updated successfully!";
                
                // Reload profile data
                var updatedProfile = await _profileService.GetProfileAsync(userId.Value);
                if (updatedProfile != null)
                {
                    Profile = updatedProfile;
                }
            }
            else
            {
                ErrorMessage = "Failed to update profile. Username or email might already exist.";
            }

            return Page();
        }
    }
}
