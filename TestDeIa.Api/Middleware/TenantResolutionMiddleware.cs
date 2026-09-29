using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Api.Middleware;

public sealed class TenantResolutionMiddleware
{
    private const string EmpresaHeader = "X-Empresa-Id";
    private readonly RequestDelegate next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantContextAccessor, TestDeIaDbContext dbContext)
    {
        tenantContextAccessor.IsSystemContext = false;
        tenantContextAccessor.EmpresaId = null;
        tenantContextAccessor.UserId = null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                tenantContextAccessor.UserId = userId;
            }

            var tenantResolution = ResolveRequestedEmpresaId(context);
            if (!tenantResolution.IsValid)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                    new { message = "El identificador de empresa activa no tiene formato Guid valido." },
                    context.RequestAborted);
                return;
            }

            var requestedEmpresaId = tenantResolution.EmpresaId ?? ResolveDefaultEmpresaId(context);
            if (requestedEmpresaId.HasValue && tenantContextAccessor.UserId.HasValue)
            {
                tenantContextAccessor.IsSystemContext = true;

                var hasAccess = await dbContext.SecurityUserEmpresas
                    .AsNoTracking()
                    .AnyAsync(
                        current => current.SecurityUserId == tenantContextAccessor.UserId.Value && current.EmpresaId == requestedEmpresaId.Value,
                        context.RequestAborted);

                if (!hasAccess)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { message = "La empresa activa no pertenece a la sesion autenticada." }, context.RequestAborted);
                    return;
                }

                tenantContextAccessor.EmpresaId = requestedEmpresaId.Value;
                tenantContextAccessor.IsSystemContext = false;
            }
        }

        await next(context);
    }

    private static TenantHeaderResolution ResolveRequestedEmpresaId(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(EmpresaHeader, out var values))
        {
            return TenantHeaderResolution.Empty;
        }

        var rawValue = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return TenantHeaderResolution.Empty;
        }

        return Guid.TryParse(rawValue, out var empresaId)
            ? TenantHeaderResolution.Valid(empresaId)
            : TenantHeaderResolution.Invalid;
    }

    private static Guid? ResolveDefaultEmpresaId(HttpContext context)
    {
        var rawValue = context.User.FindFirstValue("default_empresa_id");
        return Guid.TryParse(rawValue, out var empresaId) ? empresaId : null;
    }

    private readonly record struct TenantHeaderResolution(Guid? EmpresaId, bool IsValid)
    {
        public static TenantHeaderResolution Empty => new(null, true);
        public static TenantHeaderResolution Invalid => new(null, false);
        public static TenantHeaderResolution Valid(Guid empresaId) => new(empresaId, true);
    }
}
