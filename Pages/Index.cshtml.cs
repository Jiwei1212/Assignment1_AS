using Assignment1.Model;
using Assignment1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly AuthDbContext _dbContext;  // Your DbContext
    private readonly Encryption _encryptionService;  // Your Encryption service
    private readonly AuditLogger _auditLogger;
    private readonly SessionTracker _sessionTracker;

    public IndexModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AuthDbContext dbContext, Encryption encryptionService, AuditLogger auditLogger, SessionTracker sessionTracker)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _encryptionService = encryptionService;
        _auditLogger = auditLogger;
        _sessionTracker = sessionTracker;
    }

    public IdentityUser CurrentUser { get; set; }
    public UserProfile CurrentUserProfile { get; set; }  // Your UserProfile
    public bool IsAuthenticated => _signInManager.IsSignedIn(User);
    public string DecryptedEmail { get; set; }
    public string DecryptedNRIC { get; set; }

    public async Task OnGetAsync()
    {
        if (IsAuthenticated) // Check if the user is signed in
        {
            CurrentUser = await _userManager.GetUserAsync(User); // Get the logged-in user

            if (CurrentUser == null)
            {
                Console.WriteLine("CurrentUser is null");
                // Handle the case where user is null, possibly log or redirect
            }

            if (CurrentUser != null)
            {
                // Check if the session is still active
                if (!_sessionTracker.IsSessionActive(CurrentUser.Id))
                {
                    await _signInManager.SignOutAsync();
                    HttpContext.Session.Clear();
                    Response.Redirect("/Login");
                    return;
                }

                CurrentUserProfile = await _dbContext.UserProfiles
                    .FirstOrDefaultAsync(u => u.UserId == CurrentUser.Id);

                if (CurrentUserProfile != null)
                {
                    try
                    {
                        DecryptedEmail = _encryptionService.DecryptData(CurrentUserProfile.Email);
                        DecryptedNRIC = _encryptionService.DecryptData(CurrentUserProfile.NRIC);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting data: {ex.Message}");
                        DecryptedEmail = "Error decrypting email";
                        DecryptedNRIC = "Error decrypting NRIC";
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("User is not authenticated");
            // Handle the case where the user is not authenticated
        }
    }

}
