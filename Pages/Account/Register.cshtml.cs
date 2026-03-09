using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSocialNetwork.Services;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Check if already logged in
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var (success, message, user) = await _authService.RegisterAsync(Input);

            if (!success)
            {
                ErrorMessage = message;
                return Page();
            }

            // Redirect to login with success message
            return RedirectToPage("/Account/Login", new { message = "Registration successful! Please login." });
        }
    }
}
