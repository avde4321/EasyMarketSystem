using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Clientes.Ports.Out;
using TestDeIa.Application.Modules.Catalogos.Ports.Out;
using TestDeIa.Application.Modules.Caja.Ports.Out;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Application.Modules.Dashboard.Ports.Out;
using TestDeIa.Application.Modules.Empleados.Ports.Out;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Application.Modules.Financiero.Ports.Out;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Infrastructure.Adapters.Out.Clientes;
using TestDeIa.Infrastructure.Adapters.Out.Catalogos;
using TestDeIa.Infrastructure.Adapters.Out.Caja;
using TestDeIa.Infrastructure.Adapters.Out.Compras;
using TestDeIa.Infrastructure.Adapters.Out.Dashboard;
using TestDeIa.Infrastructure.Adapters.Out.Empleados;
using TestDeIa.Infrastructure.Adapters.Out.Empresa;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Adapters.Out.Financiero;
using TestDeIa.Infrastructure.Adapters.Out.Inventario;
using TestDeIa.Infrastructure.Adapters.Out.Personas;
using TestDeIa.Infrastructure.Adapters.Out.Security;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Tenancy;
using TestDeIa.Infrastructure.Security;
using TestDeIa.Infrastructure.Options;

namespace TestDeIa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.Configure<SriSoapOptions>(configuration.GetSection(SriSoapOptions.SectionName));
        services.AddDbContext<TestDeIaDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPasswordHashService, Sha256PasswordHashService>();
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<ICatalogoRepository, EfCatalogoRepository>();
        services.AddScoped<ICajaSesionRepository, EfCajaSesionRepository>();
        services.AddScoped<ISecurityUserRepository, EfSecurityUserRepository>();
        services.AddScoped<ISecurityTokenGenerator, JwtSecurityTokenGenerator>();
        services.AddScoped<IClienteRepository, EfClienteRepository>();
        services.AddScoped<ICompraRepository, EfCompraRepository>();
        services.AddScoped<IEstudioMercadoRepository, EfEstudioMercadoRepository>();
        services.AddScoped<ICuentaPorPagarRepository, EfCuentaPorPagarRepository>();
        services.AddScoped<IDashboardAnalyticsRepository, EfDashboardAnalyticsRepository>();
        services.AddSingleton<ICompraBackgroundQueue, CompraBackgroundQueue>();
        services.AddSingleton<IInventarioPredictivoService, InventarioPredictivoService>();
        services.AddSingleton<IEstudioMercadoAnaliticoService, EstudioMercadoAnaliticoService>();
        services.AddScoped<IProveedorRepository, EfProveedorRepository>();
        services.AddScoped<IEmpleadoRepository, EfEmpleadoRepository>();
        services.AddScoped<IEmpresaRepository, EfEmpresaRepository>();
        services.AddScoped<IFinancieroReportesRepository, EfFinancieroReportesRepository>();
        services.AddScoped<IPersonaRepository, EfPersonaRepository>();
        services.AddScoped<IInventarioRepository, EfInventarioRepository>();
        services.AddScoped<IFacturacionRepository, EfFacturacionRepository>();
        services.AddSingleton<SriResponseParser>();
        services.AddSingleton<SriFacturaXmlSchemaValidator>();
        services.AddSingleton<SriLiquidacionCompraXmlValidator>();
        services.AddSingleton<SriXadesBesSigner>();
        services.AddHttpClient<SriSoapClient>((serviceProvider, client) =>
        {
            var soapOptions = serviceProvider.GetRequiredService<IOptions<SriSoapOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, soapOptions.TimeoutSeconds));
        });
        services.AddSingleton<IFacturaBackgroundQueue, FacturaBackgroundQueue>();
        services.AddScoped<ISriCompraProcessor, SriCompraProcessor>();
        services.AddScoped<ISriFacturaProcessor, SriFacturaProcessor>();
        services.AddHostedService<FacturacionBackgroundWorker>();
        services.AddHostedService<CompraBackgroundWorker>();

        return services;
    }
}
