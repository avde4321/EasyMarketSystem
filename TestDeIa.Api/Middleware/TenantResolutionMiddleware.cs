using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Common;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Api.Middleware;

public sealed class TenantResolutionMiddleware
{
    private const string EmpresaHeader = "X-Empresa-Id";
    private readonly RequestDelegate next;
    private readonly DeploymentSettingsOptions deployment;

    public TenantResolutionMiddleware(RequestDelegate next, IOptions<DeploymentSettingsOptions>? deployment = null)
    {
        this.next = next;
        this.deployment = deployment?.Value ?? new DeploymentSettingsOptions();
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
            if (deployment.IsOnPremise && tenantContextAccessor.UserId.HasValue)
            {
                var companies = dbContext.EmpresasEmisoras.IgnoreQueryFilters().AsNoTracking().Where(x => x.IsActive);
                if (deployment.OnPremiseEmpresaId.HasValue)
                    companies = companies.Where(x => x.Id == deployment.OnPremiseEmpresaId.Value);
                var ids = await companies.Select(x => x.Id).Take(2).ToListAsync(context.RequestAborted);
                if (ids.Count != 1)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new { message = "Configure una única empresa local activa o DeploymentSettings:OnPremiseEmpresaId." }, context.RequestAborted);
                    return;
                }
                if (tenantResolution.EmpresaId.HasValue && tenantResolution.EmpresaId.Value != ids[0])
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { message = "La empresa solicitada no corresponde a esta instalación local." }, context.RequestAborted);
                    return;
                }
                requestedEmpresaId = ids[0];
            }
            if (requestedEmpresaId.HasValue && tenantContextAccessor.UserId.HasValue)
            {
                var hasAccess = await dbContext.SecurityUserEmpresas
                    .IgnoreQueryFilters()
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
