using Assignment1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Assignment1.Pages
{
    [ValidateAntiForgeryToken]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly SessionTracker _sessionTracker;

        public LogoutModel(SignInManager<IdentityUser> signInManager, SessionTracker sessionTracker)
        {
            _signInManager = signInManager;
            _sessionTracker = sessionTracker;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // Remove session from tracker
                _sessionTracker.RemoveSession(userId);

                HttpContext.Session.Clear(); 
            }

            await _signInManager.SignOutAsync(); // Sign out
            return RedirectToPage("Login"); // Redirect to login
        }
    }

}
