using ecommerce.Attributes;
using ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;

namespace ecommerce.Handlers
{
    public class EmailConfirmedRequirementHandler : AuthorizationHandler<EmailConfirmedRequirement>
    {
        private readonly UserManager<User> _userManager;

        public EmailConfirmedRequirementHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailConfirmedRequirement requirement)
        {
            var emailClaim = context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var userEmail = emailClaim?.Value; 

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null && user.EmailConfirmed)
            {
                context.Succeed(requirement);
            }
        }
    }
}
