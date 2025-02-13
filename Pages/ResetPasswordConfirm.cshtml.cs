using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace Assignment1.Pages
{
    public class ResetPasswordConfirmModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ResetPasswordConfirmModel> _logger;

        public ResetPasswordConfirmModel(UserManager<IdentityUser> userManager, ILogger<ResetPasswordConfirmModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        public class ResetPasswordInputModel
        {

            [Required]
            [DataType(DataType.Password)]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet()
        {
            if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Invalid or expired reset token.";
                
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("reset fail");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                ErrorMessage = "No user found with that email.";
                return Page();
            }

            // Reset the password using the token
            var result = await _userManager.ResetPasswordAsync(user, Token, Input.NewPassword);
            if (result.Succeeded)
            {
                Console.WriteLine("reset pass");
                return RedirectToPage("/Login", new { successMessage = "Password reset successful. Please log in." });
            }

            ErrorMessage = "Failed to reset password. Please try again.";
            return Page();
        }
    }
}
