using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet(string? message = null)
        {
            // Check if already logged in
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToPage("/Index");
            }

            if (!string.IsNullOrEmpty(message))
            {
                SuccessMessage = message;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, message, user) = await _authService.LoginAsync(Input);

            if (!success)
            {
                ErrorMessage = message;
                return Page();
            }

            // Set session
            HttpContext.Session.SetInt32("UserId", user!.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);
            
            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                HttpContext.Session.SetString("AvatarPath", user.AvatarPath);
            }

            return RedirectToPage("/Index");
        }
    }
}
