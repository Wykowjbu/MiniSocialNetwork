using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthService _authService;

        public ChangePasswordModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public ChangePasswordViewModel Input { get; set; } = new();

        public string? SuccessMessage { get; set; }
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

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _authService.ChangePasswordAsync(userId.Value, Input);
            if (success)
            {
                SuccessMessage = "Password changed successfully!";
                Input = new ChangePasswordViewModel(); // Clear form
            }
            else
            {
                ErrorMessage = "Current password is incorrect.";
            }

            return Page();
        }
    }
}
