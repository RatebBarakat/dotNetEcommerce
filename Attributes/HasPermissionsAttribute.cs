using Microsoft.AspNetCore.Authorization;

namespace ecommerce.Attributes
{
    public class HasPermissionsAttribute : AuthorizeAttribute
    {
        public HasPermissionsAttribute(string permission) : base(permission)
        {
            
        }
    }
}
