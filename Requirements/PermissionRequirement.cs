using Microsoft.AspNetCore.Authorization;

namespace ecommerce.Requirements
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string permission;

        public PermissionRequirement(string perm) 
        {
            permission = perm;
        }
    }
}
