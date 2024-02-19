using ecommerce.Attributes;
using ecommerce.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace ecommerce.Policies
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "Permission:";

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => Task.FromResult<AuthorizationPolicy>(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy>(null);

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName.Substring(POLICY_PREFIX.Length);
                var policy = new AuthorizationPolicyBuilder().AddRequirements(new PermissionRequirement(permission)).Build();
                return Task.FromResult(policy);
            }
            else if (policyName.Equals("EmailConfirmedPolicy", StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder().AddRequirements(new EmailConfirmedRequirement()).Build();
                return Task.FromResult(policy);
            }

            return Task.FromResult<AuthorizationPolicy>(null);
        }

    }

}
