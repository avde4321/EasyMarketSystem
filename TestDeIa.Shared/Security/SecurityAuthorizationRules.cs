using System.Security.Claims;

namespace TestDeIa.Shared.Security;

public static class SecurityAuthorizationRules
{
    public static bool HasAccessToAnyPermission(ClaimsPrincipal user, IEnumerable<string> permissions)
    {
        return user.IsInRole(SecurityRoleNames.SuperAdministrador) ||
            permissions.Any(permission => user.HasClaim(SecurityClaimTypes.Permission, permission));
    }
}
