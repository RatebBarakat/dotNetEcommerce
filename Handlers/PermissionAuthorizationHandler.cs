using ecommerce.Data;
using ecommerce.Helpers;
using ecommerce.Models;
using ecommerce.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ecommerce.Handlers
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly PermissionHelper _permissionHelper;
        public PermissionAuthorizationHandler(PermissionHelper permissionHelper)
        {
            _permissionHelper = permissionHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (email is null)
            {
                return;
            }
            var hasPermission = await _permissionHelper.HasPermission(email, requirement.permission);
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
