using ecommerce.Attributes;
using ecommerce.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;

namespace ecommerce.Policies
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {   
        private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }
        const string POLICY_PREFIX = "Permission:";

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

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

            return BackupPolicyProvider.GetPolicyAsync(policyName);
        }

    }

}
