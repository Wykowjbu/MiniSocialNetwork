using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Posts
{
    public class LikesModel : PageModel
    {
        private readonly ILikeService _likeService;
        private readonly IFollowService _followService;

        public LikesModel(ILikeService likeService, IFollowService followService)
        {
            _likeService = likeService;
            _followService = followService;
        }

        public int PostId { get; set; }
        public IEnumerable<UserSearchViewModel>? Users { get; set; }
        public bool IsLoggedIn { get; set; }
        public int? CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PostId = id;
            CurrentUserId = HttpContext.Session.GetInt32("UserId");
            IsLoggedIn = CurrentUserId != null;

            Users = await _likeService.GetUsersWhoLikedPostAsync(id, CurrentUserId);
            return Page();
        }

        public async Task<IActionResult> OnPostToggleFollowAsync(int targetUserId, int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _followService.ToggleFollowAsync(userId.Value, targetUserId);
            return RedirectToPage("/Posts/Likes", new { id = postId });
        }
    }
}
