using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Security;

namespace TestDeIa.Api.Middleware;

// Defensa del backend, incluso para JWT antiguos con permisos operativos amplios.
public sealed class PlatformOwnerBoundaryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenant, TestDeIaDbContext db)
    {
        if (tenant.UserId.HasValue && await db.SecurityUserRoles.IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(x => x.UserId == tenant.UserId.Value && x.Role.Name == SecurityRoleNames.SuperAdministrador
                && x.Role.IsActive && x.User.IsActive, context.RequestAborted))
        {
            var path = context.Request.Path.Value?.TrimEnd('/') ?? "";
            var platformRoute = path.Equals("/api/v1/saas/billing/admin/payments", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/v1/saas/billing/admin/consumption", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/v1/saas/billing/admin/dashboard", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/v1/saas/billing/confirm-payment", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/api/empresa", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/security/logout", StringComparison.OrdinalIgnoreCase);
            platformRoute |= HttpMethods.IsGet(context.Request.Method) &&
                (path.StartsWith("/api/geografia/", StringComparison.OrdinalIgnoreCase)
                 || path.StartsWith("/api/catalogos/", StringComparison.OrdinalIgnoreCase)
                 || path.Equals("/api/saas/plans", StringComparison.OrdinalIgnoreCase)
                 || path.Equals("/api/v1/saas/plans", StringComparison.OrdinalIgnoreCase));
            if (!platformRoute && path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
                && !tenant.EmpresaId.HasValue)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = 403, Title = "Selecciona tu empresa operativa",
                    Detail = "La administración SaaS no permite consultar información comercial de las empresas clientes."
                }, context.RequestAborted);
                return;
            }
        }
        await next(context);
    }
}
