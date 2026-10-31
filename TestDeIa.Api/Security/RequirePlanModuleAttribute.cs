using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Saas.Ports.In;
using TestDeIa.Domain.Modules.Saas.Enums;

namespace TestDeIa.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePlanModuleAttribute(SaasModule module) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        if (http.User.Identity?.IsAuthenticated != true) { context.Result = new ChallengeResult(); return; }
        var tenant = http.RequestServices.GetRequiredService<ITenantContextAccessor>();
        try
        {
            if (!tenant.EmpresaId.HasValue)
                throw new SaaSQuotaExceededException("Seleccione una empresa para acceder al módulo.", "EMPRESA_REQUERIDA");
            await http.RequestServices.GetRequiredService<ISaaSQuotaValidationService>()
                .EnsureModuleAsync(tenant.EmpresaId.Value, module, http.RequestAborted);
        }
        catch (SaaSQuotaExceededException ex)
        {
            var problem = new ProblemDetails { Status = 403, Title = "Módulo no disponible en el plan", Detail = ex.Message };
            problem.Extensions["quotaCode"] = ex.QuotaCode;
            problem.Extensions["module"] = module.ToString();
            context.Result = new ObjectResult(problem) { StatusCode = 403 };
        }
    }
}
