using TestDeIa.Api.Messaging;
using TestDeIa.Api.Reporting;
using TestDeIa.Api.Security;

namespace TestDeIa.Api.Configuration;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddApiComposition(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(AuthorizationPolicyRegistration.Register);
        services.Configure<SmtpEmailOptions>(configuration.GetSection(SmtpEmailOptions.SectionName));
        services.AddScoped<FacturaDocumentQueryService>();
        services.AddSingleton<FacturaRideRdlcRenderer>();
        services.AddSingleton<SmtpFacturaEmailSender>();
        services.AddHostedService<FacturacionEmailNotifier>();

        return services;
    }
}
