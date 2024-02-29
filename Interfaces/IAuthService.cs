using ecommerce.Dtos;
using ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Interfaces
{
    public interface IAuthService
    {
        public Task<IActionResult> RegisterUser(RegisterUser user, bool verified = false);
        public Task<bool> CheckUserCredentials(LoginUser user);
        public Task<string?> GenerateJwtToken(string email);
        public Task<UserWithPermissionsDTO?> GetUserDetails();
        public Task<IActionResult> ConfirmEmail(string userId, string token);

    }
}