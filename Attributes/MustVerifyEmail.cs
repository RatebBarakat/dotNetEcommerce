using ecommerce.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ecommerce.Models;
using System.Security.Claims;

namespace ecommerce.Attributes
{
    public class MustVerifyEmailAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            var isEmailVerified = bool.Parse(user.FindFirstValue("IsEmailVerified"));
            if (!isEmailVerified)
            {
                context.Result = new ConflictResult();
                return;
            }
        }
    }
}
