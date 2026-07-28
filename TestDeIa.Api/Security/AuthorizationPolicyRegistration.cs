using Microsoft.AspNetCore.Authorization;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Security;

internal static class AuthorizationPolicyRegistration
{
    internal static void Register(AuthorizationOptions options)
    {
        foreach (var policy in SecurityPolicyCatalog.Policies)
        {
            var requiredPermissions = policy.Value
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            options.AddPolicy(policy.Key, builder =>
            {
                builder.RequireAuthenticatedUser();
                builder.RequireAssertion(context =>
                    requiredPermissions.Any(permission =>
                        context.User.HasClaim(SecurityClaimTypes.Permission, permission)));
            });
        }
    }
}
