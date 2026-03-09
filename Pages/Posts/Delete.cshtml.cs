using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Posts
{
    public class DeleteModel : PageModel
    {
        private readonly IPostService _postService;

        public DeleteModel(IPostService postService)
        {
            _postService = postService;
        }

        public PostViewModel? Post { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Post = await _postService.GetPostAsync(id, userId);
            if (Post == null)
            {
                return NotFound();
            }

            // Check ownership
            if (Post.UserId != userId.Value)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var success = await _postService.DeletePostAsync(id, userId.Value);
            if (success)
            {
                return RedirectToPage("/Posts/Index", new { message = "Post deleted successfully!" });
            }

            return RedirectToPage("/Posts/Index");
        }
    }
}
