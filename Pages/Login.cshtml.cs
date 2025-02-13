using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment1.Model;
using Assignment1.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Assignment1.Pages
{
    [ValidateAntiForgeryToken]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditLogger _auditLogger;
        private readonly ILogger<LoginModel> _logger;
        private readonly SessionTracker _sessionTracker;

        [BindProperty]
        public Login LModel { get; set; }

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, AuditLogger auditLogger, ILogger<LoginModel> logger, SessionTracker sessionTracker)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLogger = auditLogger;
            _logger = logger;
            _sessionTracker = sessionTracker;
        }

        public void OnGet()
        {
       
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Sanitize the email and password to prevent XSS attacks
            var sanitizedEmail = SanitizeInput(LModel.Email);
            var sanitizedPassword = SanitizeInput(LModel.Password);

            var user = await _signInManager.UserManager.Users
                .Where(u => u.Email == sanitizedEmail)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                _auditLogger.Log(user.Id, $"Failed attempts: {user.AccessFailedCount}, LockoutEnd: {user.LockoutEnd} ");

                if (user.LockoutEnd > DateTimeOffset.Now)
                {
                    ViewData["Message"] = $"Your account is locked. Please try again after {user.LockoutEnd}.";
                    return Page();
                }

                if (_sessionTracker.IsSessionActive(user.Id))
                {
                    _logger.LogWarning($"User {user.UserName} has an active session already. Preventing new login.");
                    ModelState.AddModelError("", "You are already logged in from another device. Please log out from the other device first.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(sanitizedEmail, sanitizedPassword, LModel.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} successfully logged in.");
                    _auditLogger.Log(user.Id, "User logged in.");


                    _sessionTracker.AddSession(user.Id, HttpContext.Session.Id);

                    return RedirectToPage("Index"); // Proceed with login
                }
                else
                {
                    _auditLogger.Log(user.Id, "Failed login attempt.");
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return Page();
                }
            }
            else
            {
                _logger.LogWarning($"User with email {sanitizedEmail} not found.");
                ModelState.AddModelError("", "User not found.");
                return Page();
            }

        }

        // Helper method to sanitize input to avoid XSS
        private string SanitizeInput(string input)
        {
            return Regex.Replace(input, @"<.*?>", string.Empty);
        }

        // Email format validation
        private bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }

}
