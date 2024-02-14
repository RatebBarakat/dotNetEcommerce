using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Interfaces
{
    public interface IAuthService
    {
        public Task<IActionResult> RegisterUser(RegisterUser user);
        public Task<bool> CheckUserCredentials(LoginUser user);
        public Task<string?> GenerateJwtToken(string email);
        public Task<object?> GetUserDetails();
        public Task<IActionResult> ConfirmEmail(string userId, string token);

    }
}