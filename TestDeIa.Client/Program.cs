using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TestDeIa.Client;
using TestDeIa.Client.Options;
using TestDeIa.Client.Security;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Caja;
using TestDeIa.Client.Services.ActivosFijos;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Client.Services.Contabilidad;
using TestDeIa.Client.Services.Dashboard;
using TestDeIa.Client.Services.Empleados;
using TestDeIa.Client.Services.Empresa;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Client.Services.Financiero;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Client.Services.Personas;
using TestDeIa.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiOptions = builder.Configuration.GetSection("Api").Get<ApiOptions>() ?? new ApiOptions();

builder.Services.AddAuthorizationCore(AuthorizationPolicyRegistration.Register);
builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<PopupNotificationService>();
builder.Services.AddScoped<EmpresaSessionService>();
builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<TokenAuthenticationStateProvider>());
builder.Services.AddScoped<AuthenticationHeaderHandler>();
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiOptions.BaseUrl);
})
.AddHttpMessageHandler<AuthenticationHeaderHandler>();
builder.Services.AddScoped(provider => provider.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));
builder.Services.AddScoped<SecurityApiClient>();
builder.Services.AddScoped<CatalogosApiClient>();
builder.Services.AddScoped<CajaApiClient>();
builder.Services.AddScoped<ActivosFijosApiClient>();
builder.Services.AddScoped<ClientesApiClient>();
builder.Services.AddScoped<ComprasApiClient>();
builder.Services.AddScoped<ContabilidadApiClient>();
builder.Services.AddScoped<EstudioMercadoApiClient>();
builder.Services.AddScoped<CuentasPorPagarApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<ProveedoresApiClient>();
builder.Services.AddScoped<EmpleadosApiClient>();
builder.Services.AddScoped<EmpresaApiClient>();
builder.Services.AddScoped<FacturacionApiClient>();
builder.Services.AddScoped<FinancieroApiClient>();
builder.Services.AddScoped<PersonasApiClient>();
builder.Services.AddScoped<InventarioApiClient>();

await builder.Build().RunAsync();

