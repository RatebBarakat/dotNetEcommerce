using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Interfaces
{
    public interface IEmailSender
    {
        Task<IActionResult> SendAsync();
    }
}