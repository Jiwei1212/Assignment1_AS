using SendGrid.Helpers.Mail;
using SendGrid;

namespace Assignment1.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _sendGridApiKey;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridApiKey = _configuration["SendGrid:ApiKey"];
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("foojiwei1052@gmail.com", "Ace Job Agency");
            var subject = "Password Reset Request";
            var to = new EmailAddress(email);
            var plainTextContent = $"To reset your password, click on the following link: {resetLink}";
            var htmlContent = $"<strong>To reset your password, click on the following link:</strong> <a href='{resetLink}'>Reset Password</a>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);
        }
    }
}
