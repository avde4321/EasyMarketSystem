using Microsoft.AspNetCore.Cors.Infrastructure;

namespace TestDeIa.Api.Configuration;

public static class CorsPolicyConfiguration
{
    public const string BlazorClientPolicy = "BlazorClient";

    public static void Register(IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddCors(options => options.AddPolicy(BlazorClientPolicy, policy =>
        {
            if (isDevelopment)
            {
                policy.SetIsOriginAllowed(IsLoopbackOrigin);
            }
            else
            {
                policy.WithOrigins(GetProductionOrigins(configuration));
            }

            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .SetPreflightMaxAge(TimeSpan.FromHours(1));
        }));
    }

    public static string[] GetProductionOrigins(IConfiguration configuration)
    {
        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (configuredOrigins.Length == 0)
        {
            throw new InvalidOperationException("Configure Cors:AllowedOrigins con los dominios HTTPS oficiales para ambientes productivos.");
        }

        var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var configuredOrigin in configuredOrigins)
        {
            if (!Uri.TryCreate(configuredOrigin?.Trim(), UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || uri.IsLoopback
                || uri.UserInfo.Length > 0
                || uri.AbsolutePath != "/"
                || !string.IsNullOrEmpty(uri.Query)
                || !string.IsNullOrEmpty(uri.Fragment)
                || configuredOrigin!.Contains('*', StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"El origen CORS productivo '{configuredOrigin}' no es válido. Use únicamente un origen HTTPS explícito, sin rutas ni comodines.");
            }

            origins.Add(uri.GetLeftPart(UriPartial.Authority));
        }

        return origins.ToArray();
    }

    public static bool IsLoopbackOrigin(string? origin)
    {
        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.IsLoopback
            && uri.UserInfo.Length == 0
            && uri.AbsolutePath == "/"
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment);
    }
}
