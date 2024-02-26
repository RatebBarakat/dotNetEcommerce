using ecommerce.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using System.IO;
using System.Threading.Tasks;

namespace ecommerce.Emails
{
    public class SendEmailVerificationLink
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SendEmailVerificationLink(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> SendAsync(string email, string url)
        {
            var emailContent = new MimeMessage();
            emailContent.From.Add(MailboxAddress.Parse("cyrus.weissnat@ethereal.email"));
            emailContent.To.Add(MailboxAddress.Parse(email));
            emailContent.Subject = "test";

            var templatesFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
            var filePath = Path.Combine(templatesFolderPath, "EmailConfirmation.html");

            using (StreamReader stream = File.OpenText(filePath))
            {
                var htmlContent = await stream.ReadToEndAsync();
                htmlContent = string.Format(htmlContent, url, "Please confirm your email");

                emailContent.Body = new TextPart(TextFormat.Html) { Text = htmlContent };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.ethereal.email", 587, false);
                await smtp.AuthenticateAsync("cyrus.weissnat@ethereal.email", "NyCF2ndsHB18ZQpBxz");
                await smtp.SendAsync(emailContent);
                await smtp.DisconnectAsync(true);

                return new OkResult();
            }
        }
    }
}