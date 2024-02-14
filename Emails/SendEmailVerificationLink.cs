using ecommerce.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

namespace ecommerce.Emails
{
    public class SendEmailVerificationLink
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public static async Task<IActionResult> SendAsync(string email,string url)
        {
            var EMailContent = new MimeMessage();
            EMailContent.From.Add(MailboxAddress.Parse("domenick.ebert@ethereal.email"));
            EMailContent.To.Add(MailboxAddress.Parse(email));
            EMailContent.Subject = "test";
            EMailContent.Body = new TextPart(TextFormat.Html) { Text = $"click here to verify yout email <a href={url}>link</a>" };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.ethereal.email", 587, false);
            await smtp.AuthenticateAsync("domenick.ebert@ethereal.email", "6qQ6tXD61ug5fUTDEw");
            await smtp.SendAsync(EMailContent);
            await smtp.DisconnectAsync(true);
            return new OkResult();
        }
    }
}