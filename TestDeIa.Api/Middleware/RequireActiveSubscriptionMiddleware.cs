using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;

namespace TestDeIa.Api.Middleware;

public sealed class RequireActiveSubscriptionMiddleware(
    RequestDelegate next,
    ILogger<RequireActiveSubscriptionMiddleware> logger,
    IOptions<DeploymentSettingsOptions>? deployment = null)
{
    private static readonly PathString[] ExemptPrefixes =
    [
        new("/health"),
        new("/api/health"),
        new("/api/security/login"),
        new("/api/empresa"),
        new("/api/geografia"),
        new("/api/v1/auth"),
        new("/api/v1/saas/plans"),
        new("/api/v1/saas/billing/checkout"),
        new("/api/saas/plans"),
        new("/api/saas/subscription-status"),
        new("/api/saas/billing/checkout"),
        new("/api/pagos-digitales/webhook/payphone")
    ];

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextAccessor tenantContextAccessor,
        ISaaSQuotaValidationService quotaValidationService)
    {
        if (!(deployment?.Value ?? new DeploymentSettingsOptions()).SubscriptionEnabled || ShouldSkip(context))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        if (!tenantContextAccessor.EmpresaId.HasValue)
        {
            await next(context);
            return;
        }

        try
        {
            await quotaValidationService.EnsureActiveSubscriptionAsync(
                tenantContextAccessor.EmpresaId.Value,
                context.RequestAborted);
        }
        catch (SubscriptionRequiredException exception)
        {
            logger.LogWarning(
                exception,
                "Request bloqueado por suscripcion inactiva. EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}, Path: {Path}",
                tenantContextAccessor.EmpresaId,
                tenantContextAccessor.UserId,
                context.Request.Path);

            await WriteSubscriptionProblemAsync(context, exception);
            return;
        }

        await next(context);
    }

    private static bool ShouldSkip(HttpContext context)
    {
        // Administración de plataforma: el endpoint y el servicio exigen SuperAdministrador activo.
        if (HttpMethods.IsGet(context.Request.Method) && (context.Request.Path.Equals("/api/v1/saas/billing/admin/payments", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.Equals("/api/v1/saas/billing/admin/consumption", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.Equals("/api/v1/saas/billing/admin/dashboard", StringComparison.OrdinalIgnoreCase))) return true;
        if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path.Equals("/api/v1/saas/billing/portal", StringComparison.OrdinalIgnoreCase)) return true;
        // Se permite pagar/confirmar aun con suscripción vencida; la autorización del endpoint sigue activa.
        if (HttpMethods.IsPost(context.Request.Method) &&
            (context.Request.Path.Equals("/api/v1/saas/billing/register-transfer", StringComparison.OrdinalIgnoreCase) ||
             context.Request.Path.Equals("/api/v1/saas/billing/confirm-payment", StringComparison.OrdinalIgnoreCase)))
            return true;
        if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path.StartsWithSegments("/api/catalogos"))
        {
            return true;
        }
        return ExemptPrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task WriteSubscriptionProblemAsync(HttpContext context, SubscriptionRequiredException exception)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status402PaymentRequired,
            Title = "Suscripción Inactiva o Vencida",
            Detail = exception.Message,
            Type = "https://easymarket.local/problems/subscription-required",
            Instance = context.Request.Path
        };

        problem.Extensions["code"] = exception.Code;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, options: null,
            contentType: "application/problem+json", cancellationToken: context.RequestAborted);
    }
}
