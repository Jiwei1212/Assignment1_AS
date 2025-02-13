using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Assignment1.Model;
using Assignment1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class ChangePasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly SessionTracker _sessionTracker;
    private readonly AuthDbContext _dbContext;

    public ChangePasswordModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, SessionTracker sessionTracker, AuthDbContext authDbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _sessionTracker = sessionTracker;
        _dbContext = authDbContext;
    }

    [BindProperty]
    public ChangePasswordInputModel Input { get; set; } = new ChangePasswordInputModel();

    public string SuccessMessage { get; set; }
    public string ErrorMessage { get; set; }

    public class ChangePasswordInputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 12 characters.")]
        // Regular expression to ensure at least one uppercase, one lowercase, one number, and one special character
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Invalid input.";
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Login");
        }

        var userId = user.Id;

        var passwordHistory = await _dbContext.PasswordHistories
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.ChangedAt)
            .Take(2)
            .ToListAsync();

        if (passwordHistory.Count > 0)
        {
            var lastPasswordChange = passwordHistory.First().ChangedAt;

            // Check if the password is older than the minimum allowed age
            if (DateTime.UtcNow - lastPasswordChange < PasswordPolicy.MinPasswordAge)
            {
                ErrorMessage = $"You must wait at least {PasswordPolicy.MinPasswordAge.Days} days before changing your password.";
                return Page();
            }

            // Check if the password is older than the maximum allowed age
            if (DateTime.UtcNow - lastPasswordChange > PasswordPolicy.MaxPasswordAge)
            {
                ErrorMessage = $"Your password has expired. Please change your password immediately.";
                return Page();
            }
        }

        foreach (var oldPassword in passwordHistory)
        {
            var isReused = _userManager.PasswordHasher.VerifyHashedPassword(user, oldPassword.HashedPassword, Input.NewPassword);
            if (isReused == PasswordVerificationResult.Success)
            {
                ErrorMessage = "You cannot reuse your last two passwords. Please choose a new password.";
                return Page();
            }
        }

        var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);

        if (result.Succeeded)
        {
            // ✅ Store new password in history
            var hashedNewPassword = _userManager.PasswordHasher.HashPassword(user, Input.NewPassword);
            var newPasswordEntry = new PasswordHistory { UserId = userId, HashedPassword = hashedNewPassword };
            _dbContext.PasswordHistories.Add(newPasswordEntry);
            await _dbContext.SaveChangesAsync();

            // ✅ Keep only last 2 passwords
            var allPasswords = await _dbContext.PasswordHistories
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.ChangedAt)
                .ToListAsync();

            if (allPasswords.Count > 2)
            {
                _dbContext.PasswordHistories.Remove(allPasswords.Last()); // Remove oldest entry
                await _dbContext.SaveChangesAsync();
            }

            // ✅ Remove session & sign out
            _sessionTracker.RemoveSession(userId);
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();

            return RedirectToPage("/Login", new { successMessage = "Password changed successfully. Please log in again." });
        }
        else
        {
            ErrorMessage = "Failed to change password. Please check your current password and try again.";
            return Page();
        }
    }
}
