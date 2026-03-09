using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Posts
{
    public class EditModel : PageModel
    {
        private readonly IPostService _postService;

        public EditModel(IPostService postService)
        {
            _postService = postService;
        }

        [BindProperty]
        public PostViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var post = await _postService.GetPostAsync(id, userId);
            if (post == null)
            {
                return NotFound();
            }

            // Check ownership
            if (post.UserId != userId.Value)
            {
                return Forbid();
            }

            Input = post;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Remove validation for optional fields
            ModelState.Remove("Input.ImageFile");
            ModelState.Remove("Input.Username");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get current post to check ownership
            var currentPost = await _postService.GetPostAsync(Input.PostId, userId);
            if (currentPost == null || currentPost.UserId != userId.Value)
            {
                ErrorMessage = "You don't have permission to edit this post.";
                return Page();
            }

            // Handle image upload
            if (Input.ImageFile != null && Input.ImageFile.Length > 0)
            {
                var imagePath = await _postService.UploadPostImageAsync(Input.ImageFile, userId.Value);
                if (imagePath != null)
                {
                    Input.ImagePath = imagePath;
                }
            }
            else
            {
                // Keep existing image
                Input.ImagePath = currentPost.ImagePath;
            }

            var success = await _postService.UpdatePostAsync(Input, userId.Value);
            if (success)
            {
                return RedirectToPage("/Posts/Index", new { message = "Post updated successfully!" });
            }

            ErrorMessage = "Failed to update post.";
            return Page();
        }
    }
}
