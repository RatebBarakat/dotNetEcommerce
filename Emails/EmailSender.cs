using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;

namespace ecommerce.Emails
{
    public abstract class EmailSender
    {
        public MimeMessage message = new MimeMessage();
        public async Task<OkResult> SendEmailAsync(string to, string subject, TextPart html, string? from = null)
        {
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = html;

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.ethereal.email", 587, false);
            await smtp.AuthenticateAsync("cary.kemmer@ethereal.email", "p143ejA52XKnU7kT2t");
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            return new OkResult();
        }
    }
}
