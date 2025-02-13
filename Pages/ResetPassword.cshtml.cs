using Assignment1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Assignment1.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailService _emailService;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(UserManager<IdentityUser> userManager, EmailService emailService, ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public string Email { get; set; }

        public string Message { get; set; }

        // Method to handle the request for sending a password reset email
        public async Task<IActionResult> OnPostRequestResetAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Email is required.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                Message = "No user found with that email.";
                return Page();
            }

            // Generate a password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create the reset link
            var resetLink = Url.Page(
                "/ResetPasswordConfirm",
                pageHandler: null,
                values: new { token, email = Email },
                protocol: Request.Scheme);

            // Send the email
            await _emailService.SendPasswordResetEmailAsync(Email, resetLink);

            Message = "Password reset email sent. Please check your inbox.";
            return Page();
        }
    }
}
