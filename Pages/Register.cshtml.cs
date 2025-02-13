using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Assignment1.Model; // ✅ Import UserProfile and AuthDbContext
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using Assignment1.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web;

namespace Assignment1.Pages
{
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuthDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly Encryption _encryption;

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AuthDbContext dbContext, IConfiguration configuration, HttpClient httpClient)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _configuration = configuration;
            _httpClient = httpClient;
            _encryption = new Encryption(configuration);
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
            // Get reCAPTCHA response from the form
            var recaptchaResponse = Request.Form["g-recaptcha-response"];

            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                ModelState.AddModelError(string.Empty, "Please complete the reCAPTCHA.");
                return Page();
            }

            // Verify reCAPTCHA response with Google API
            var secretKey = _configuration["reCAPTCHA:SecretKey"];
            var verifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}";

            var response = await _httpClient.PostAsync(verifyUrl, null);
            var responseContent = await response.Content.ReadAsStringAsync();

            var captchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(responseContent);

            if (!captchaResult.Success || captchaResult.Score < 0.5)  // Optional: Adjust score threshold
            {
                ModelState.AddModelError(string.Empty, "reCAPTCHA verification failed.");
                return Page();
            }

            // Check if the email already exists
            var existingUser = await _userManager.FindByEmailAsync(RModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("RModel.Email", "This email is already registered.");
                return Page();
            }

            var user = new IdentityUser
            {
                UserName = RModel.Email,
                Email = RModel.Email
            };

            var result = await _userManager.CreateAsync(user, RModel.Password);

            if (result.Succeeded)
            {
                // Save additional user details in UserProfile table
                await SaveUserDetails(user.Id, RModel);

                // Sign the user in after successful registration
                await _signInManager.SignInAsync(user, false);
                return RedirectToPage("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return Page();
        }

        private async Task SaveUserDetails(string userId, Register model)
        {
            // HTML encode the user inputs
            var encodedFirstName = HttpUtility.HtmlEncode(model.FirstName);
            var encodedLastName = HttpUtility.HtmlEncode(model.LastName);
            var encodedWhoAmI = HttpUtility.HtmlEncode(model.WhoAmI);

            var encryptedNRIC = _encryption.EncryptData(model.NRIC);
            var encryptedEmail = _encryption.EncryptData(model.Email);
            string resumeFilePath = await SaveResumeFile(model.Resume);

            var userProfile = new UserProfile
            {
                UserId = userId,
                FirstName = encodedFirstName,  // Encoded FirstName
                LastName = encodedLastName,    // Encoded LastName
                Gender = model.Gender,
                Email = encryptedEmail,          // Encoded Email
                NRIC = encryptedNRIC,
                DateOfBirth = model.DateOfBirth,
                WhoAmI = encodedWhoAmI,        // Encoded WhoAmI
                ResumePath = resumeFilePath
            };

            _dbContext.UserProfiles.Add(userProfile);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> SaveResumeFile(IFormFile resume)
        {
            if (resume == null || resume.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/resumes");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, $"{Guid.NewGuid()}{Path.GetExtension(resume.FileName)}");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await resume.CopyToAsync(stream);
            }

            return filePath;
        }

        public class RecaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("score")]
            public float Score { get; set; }

            [JsonProperty("action")]
            public string Action { get; set; }

            [JsonProperty("error-codes")]
            public string[] ErrorCodes { get; set; }
        }
    }
}
