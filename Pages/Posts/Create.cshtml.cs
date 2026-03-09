using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Posts
{
    public class CreateModel : PageModel
    {
        private readonly IPostService _postService;

        public CreateModel(IPostService postService)
        {
            _postService = postService;
        }

        [BindProperty]
        public PostViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

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

            // Handle image upload
            if (Input.ImageFile != null && Input.ImageFile.Length > 0)
            {
                var imagePath = await _postService.UploadPostImageAsync(Input.ImageFile, userId.Value);
                if (imagePath != null)
                {
                    Input.ImagePath = imagePath;
                }
            }

            await _postService.CreatePostAsync(Input, userId.Value);

            return RedirectToPage("/Posts/Index", new { message = "Post created successfully!" });
        }
    }
}
