using Microsoft.AspNetCore.Mvc;
using TestDeIa.Application.Common;

namespace TestDeIa.Api.Middleware;

public sealed class SaaSQuotaExceptionMiddleware(RequestDelegate next, ILogger<SaaSQuotaExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (SaaSQuotaExceededException exception)
        {
            logger.LogWarning(
                exception,
                "Operacion bloqueada por cuota SaaS. Codigo: {QuotaCode}, TraceId: {TraceId}",
                exception.QuotaCode,
                context.TraceIdentifier);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Límite del plan SaaS alcanzado",
                Detail = exception.Message,
                Type = "https://easymarket.local/problems/saas-quota-exceeded",
                Instance = context.Request.Path
            };

            problem.Extensions["quotaCode"] = exception.QuotaCode;
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
