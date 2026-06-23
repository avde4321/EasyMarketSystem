using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Clientes.Ports.Out;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Application.Modules.Facturacion.Ports.Out;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Application.Modules.Personas.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Infrastructure.Adapters.Out.Clientes;
using TestDeIa.Infrastructure.Adapters.Out.Empresa;
using TestDeIa.Infrastructure.Adapters.Out.Facturacion;
using TestDeIa.Infrastructure.Adapters.Out.Inventario;
using TestDeIa.Infrastructure.Adapters.Out.Personas;
using TestDeIa.Infrastructure.Adapters.Out.Security;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<TestDeIaDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IPasswordHashService, Sha256PasswordHashService>();
        services.AddScoped<ISecurityUserRepository, EfSecurityUserRepository>();
        services.AddScoped<ISecurityTokenGenerator, JwtSecurityTokenGenerator>();
        services.AddScoped<IClienteRepository, EfClienteRepository>();
        services.AddScoped<IEmpresaRepository, EfEmpresaRepository>();
        services.AddScoped<IPersonaRepository, EfPersonaRepository>();
        services.AddScoped<IInventarioRepository, EfInventarioRepository>();
        services.AddScoped<IFacturacionRepository, EfFacturacionRepository>();
        services.AddSingleton<IFacturaBackgroundQueue, FacturaBackgroundQueue>();
        services.AddScoped<ISriFacturaProcessor, SimulatedSriFacturaProcessor>();
        services.AddHostedService<FacturacionBackgroundWorker>();

        return services;
    }
}
