using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Posts
{
    public class IndexModel : PageModel
    {
        private readonly IPostService _postService;

        public IndexModel(IPostService postService)
        {
            _postService = postService;
        }

        public IEnumerable<PostViewModel>? Posts { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? message = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            Posts = await _postService.GetUserPostsAsync(userId.Value, userId);
            return Page();
        }
    }
}
