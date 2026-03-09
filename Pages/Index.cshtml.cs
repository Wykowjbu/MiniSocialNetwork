using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly IProfileService _profileService;

        public IndexModel(IPostService postService, IProfileService profileService)
        {
            _postService = postService;
            _profileService = profileService;
        }

        public IEnumerable<PostViewModel>? Posts { get; set; }
        public ProfileViewModel? CurrentProfile { get; set; }
        public bool IsLoggedIn { get; set; }
        public string? CurrentUsername { get; set; }
        public string? CurrentUserAvatar { get; set; }
        public string? SuccessMessage { get; set; }
        public string FeedType { get; set; } = "all";

        [BindProperty]
        public string? NewPostContent { get; set; }

        public async Task OnGetAsync(string? message = null, string feed = "all")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            IsLoggedIn = userId != null;
            FeedType = feed;

            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            if (IsLoggedIn)
            {
                CurrentUsername = HttpContext.Session.GetString("Username");
                CurrentUserAvatar = HttpContext.Session.GetString("AvatarPath");
                CurrentProfile = await _profileService.GetProfileAsync(userId!.Value);

                // Get feed based on type
                if (feed == "following")
                {
                    Posts = await _postService.GetFollowingFeedAsync(userId.Value, 1, 20);
                }
                else
                {
                    Posts = await _postService.GetHomeFeedAsync(1, 20, userId);
                }
            }
            else
            {
                Posts = await _postService.GetHomeFeedAsync(1, 20, null);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!string.IsNullOrWhiteSpace(NewPostContent))
            {
                var postViewModel = new PostViewModel { Content = NewPostContent };

                // Handle image upload
                var imageFile = Request.Form.Files.GetFile("NewPostImage");
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imagePath = await _postService.UploadPostImageAsync(imageFile, userId.Value);
                    if (imagePath != null)
                    {
                        postViewModel.ImagePath = imagePath;
                    }
                }

                await _postService.CreatePostAsync(postViewModel, userId.Value);
            }

            return RedirectToPage("/Index", new { message = "Post created successfully!" });
        }

        public async Task<IActionResult> OnPostLikeAsync(int postId, string feedType = "all")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _postService.ToggleLikeAsync(postId, userId.Value);
            return RedirectToPage("/Index", new { feed = feedType });
        }
    }
}
