using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Users
{
    public class SearchModel : PageModel
    {
        private readonly ISearchService _searchService;
        private readonly IFollowService _followService;

        public SearchModel(ISearchService searchService, IFollowService followService)
        {
            _searchService = searchService;
            _followService = followService;
        }

        public string? Query { get; set; }
        public IEnumerable<UserSearchViewModel>? Results { get; set; }
        public bool IsLoggedIn { get; set; }
        public int? CurrentUserId { get; set; }

        public async Task OnGetAsync(string? q)
        {
            Query = q;
            CurrentUserId = HttpContext.Session.GetInt32("UserId");
            IsLoggedIn = CurrentUserId != null;

            if (!string.IsNullOrWhiteSpace(q))
            {
                Results = await _searchService.SearchUsersAsync(q, CurrentUserId);
            }
        }

        public async Task<IActionResult> OnPostToggleFollowAsync(int targetUserId, string? query)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _followService.ToggleFollowAsync(userId.Value, targetUserId);
            return RedirectToPage("/Users/Search", new { q = query });
        }
    }
}
