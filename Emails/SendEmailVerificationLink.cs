using ecommerce.Interfaces;
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
    public class SendEmailVerificationLink : EmailSender
    {

        public async Task<IActionResult> SendAsync(string email, string url)
        {
            var templatesFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
            var filePath = Path.Combine(templatesFolderPath, "EmailConfirmation.html");

            using (StreamReader stream = File.OpenText(filePath))
            {
                var htmlContent = await stream.ReadToEndAsync();
                htmlContent = string.Format(htmlContent, url, "Please confirm your email");

                var html = new TextPart(TextFormat.Html) { Text = htmlContent };

                await base.SendEmailAsync(email, "email confirm", html);

                return new OkResult();
            }
        }
    }
}