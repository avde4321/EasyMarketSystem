using Microsoft.Extensions.DependencyInjection;
using TestDeIa.Application.Modules.Catalogos.Ports.In;
using TestDeIa.Application.Modules.Catalogos.UseCases;
using TestDeIa.Application.Modules.Caja.Ports.In;
using TestDeIa.Application.Modules.Caja.UseCases;
using TestDeIa.Application.Modules.Clientes.Ports.In;
using TestDeIa.Application.Modules.Clientes.UseCases;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Application.Modules.Compras.UseCases;
using TestDeIa.Application.Modules.Dashboard.Ports.In;
using TestDeIa.Application.Modules.Dashboard.UseCases;
using TestDeIa.Application.Modules.Empleados.Ports.In;
using TestDeIa.Application.Modules.Empleados.UseCases;
using TestDeIa.Application.Modules.Empresa.Ports.In;
using TestDeIa.Application.Modules.Empresa.UseCases;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.Facturacion.UseCases;
using TestDeIa.Application.Modules.Financiero.Ports.In;
using TestDeIa.Application.Modules.Financiero.Services;
using TestDeIa.Application.Modules.Financiero.UseCases;
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
        services.AddScoped<ICajaSesionUseCase, CajaSesionUseCase>();
        services.AddScoped<IClienteUseCase, ClienteUseCase>();
        services.AddScoped<ICompraUseCase, CompraUseCase>();
        services.AddScoped<IEstudioMercadoUseCase, EstudioMercadoUseCase>();
        services.AddScoped<IProveedorUseCase, ProveedorUseCase>();
        services.AddScoped<ICompraBackgroundCoordinator, CompraBackgroundCoordinator>();
        services.AddScoped<IDashboardUseCase, DashboardUseCase>();
        services.AddScoped<IEmpleadoUseCase, EmpleadoUseCase>();
        services.AddScoped<IEmpresaUseCase, EmpresaUseCase>();
        services.AddScoped<IAnalizadorFiscalIAService, AnalizadorFiscalIAService>();
        services.AddScoped<IFinancieroReportesUseCase, FinancieroReportesUseCase>();
        services.AddScoped<IPersonaUseCase, PersonaUseCase>();
        services.AddScoped<IInventarioUseCase, InventarioUseCase>();
        services.AddScoped<IFacturacionUseCase, FacturacionUseCase>();
        services.AddScoped<IFacturacionBackgroundCoordinator, FacturacionBackgroundCoordinator>();

        return services;
    }
}
