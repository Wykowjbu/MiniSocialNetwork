using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Posts
{
    public class DetailsModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;

        public DetailsModel(IPostService postService, ICommentService commentService)
        {
            _postService = postService;
            _commentService = commentService;
        }

        public PostViewModel? Post { get; set; }
        public IEnumerable<CommentViewModel>? Comments { get; set; }
        public bool IsLoggedIn { get; set; }
        public string? CurrentUserAvatar { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            IsLoggedIn = userId != null;
            CurrentUserAvatar = HttpContext.Session.GetString("AvatarPath");

            Post = await _postService.GetPostAsync(id, userId);
            if (Post == null)
            {
                return NotFound();
            }

            Comments = await _commentService.GetCommentsByPostAsync(id, userId);
            return Page();
        }

        public async Task<IActionResult> OnPostLikeAsync(int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _postService.ToggleLikeAsync(postId, userId.Value);
            return RedirectToPage("/Posts/Details", new { id = postId });
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int postId, string commentContent)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!string.IsNullOrWhiteSpace(commentContent))
            {
                await _commentService.CreateCommentAsync(postId, userId.Value, commentContent);
            }

            return RedirectToPage("/Posts/Details", new { id = postId });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId, int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _commentService.DeleteCommentAsync(commentId, userId.Value);
            return RedirectToPage("/Posts/Details", new { id = postId });
        }
    }
}
