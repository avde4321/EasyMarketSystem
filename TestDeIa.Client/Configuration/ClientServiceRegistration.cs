using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TestDeIa.Client.Options;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services;
using TestDeIa.Client.Services.ActivosFijos;
using TestDeIa.Client.Services.Caja;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Client.Services.Contabilidad;
using TestDeIa.Client.Services.Dashboard;
using TestDeIa.Client.Services.Empleados;
using TestDeIa.Client.Services.Empresa;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Client.Services.Financiero;
using TestDeIa.Client.Services.Geografia;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Client.Services.Reporteria;

namespace TestDeIa.Client.Configuration;

public static class ClientServiceRegistration
{
    public static IServiceCollection AddClientComposition(this IServiceCollection services, WebAssemblyHostConfiguration configuration)
    {
        var apiOptions = configuration.GetSection("Api").Get<ApiOptions>() ?? new ApiOptions();

        services.AddAuthorizationCore(AuthorizationPolicyRegistration.Register);
        services.AddScoped<TokenStorageService>();
        services.AddScoped<PopupNotificationService>();
        services.AddScoped<EmpresaSessionService>();
        services.AddScoped<TokenAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(provider =>
            provider.GetRequiredService<TokenAuthenticationStateProvider>());
        services.AddScoped<AuthenticationHeaderHandler>();
        services.AddHttpClient("Api", client =>
        {
            client.BaseAddress = new Uri(apiOptions.BaseUrl);
        })
        .AddHttpMessageHandler<AuthenticationHeaderHandler>();

        services.AddScoped(provider => provider.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));
        services.AddScoped<SecurityApiClient>();
        services.AddScoped<CatalogosApiClient>();
        services.AddScoped<CajaApiClient>();
        services.AddScoped<ActivosFijosApiClient>();
        services.AddScoped<ClientesApiClient>();
        services.AddScoped<ComprasApiClient>();
        services.AddScoped<ContabilidadApiClient>();
        services.AddScoped<EstudioMercadoApiClient>();
        services.AddScoped<CuentasPorPagarApiClient>();
        services.AddScoped<DashboardApiClient>();
        services.AddScoped<ProveedoresApiClient>();
        services.AddScoped<EmpleadosApiClient>();
        services.AddScoped<EmpresaApiClient>();
        services.AddScoped<CertificadosDigitalesApiClient>();
        services.AddScoped<FacturacionApiClient>();
        services.AddScoped<FinancieroApiClient>();
        services.AddScoped<GeografiaApiClient>();
        services.AddScoped<PersonasApiClient>();
        services.AddScoped<ReporteriaVentasApiClient>();
        services.AddScoped<InventarioApiClient>();

        return services;
    }
}
