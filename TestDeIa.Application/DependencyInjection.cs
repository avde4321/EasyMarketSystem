using Microsoft.Extensions.DependencyInjection;
using TestDeIa.Application.Modules.Catalogos.Ports.In;
using TestDeIa.Application.Modules.Catalogos.UseCases;
using TestDeIa.Application.Modules.Clientes.Ports.In;
using TestDeIa.Application.Modules.Clientes.UseCases;
using TestDeIa.Application.Modules.Empleados.Ports.In;
using TestDeIa.Application.Modules.Empleados.UseCases;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Application.Modules.Empresa.UseCases;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.UseCases;
using TestDeIa.Application.Modules.Inventario.Ports.In;
using TestDeIa.Application.Modules.Inventario.UseCases;
using TestDeIa.Application.Modules.Personas.Ports.In;
using TestDeIa.Application.Modules.Personas.UseCases;
using TestDeIa.Application.Modules.Security.Ports.In;
using TestDeIa.Application.Modules.Security.UseCases;

namespace TestDeIa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<ISecurityManagementUseCase, SecurityManagementUseCase>();
        services.AddScoped<ICatalogoUseCase, CatalogoUseCase>();
        services.AddScoped<IClienteUseCase, ClienteUseCase>();
        services.AddScoped<IEmpleadoUseCase, EmpleadoUseCase>();
        services.AddScoped<IEmpresaUseCase, EmpresaUseCase>();
        services.AddScoped<IPersonaUseCase, PersonaUseCase>();
        services.AddScoped<IInventarioUseCase, InventarioUseCase>();
        services.AddScoped<IFacturacionUseCase, FacturacionUseCase>();
        services.AddScoped<IFacturacionBackgroundCoordinator, FacturacionBackgroundCoordinator>();

        return services;
    }
}
