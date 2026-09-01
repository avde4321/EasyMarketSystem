using Microsoft.Extensions.DependencyInjection;
using TestDeIa.Application.Modules.Catalogos.Ports.In;
using TestDeIa.Application.Modules.Catalogos.UseCases;
using TestDeIa.Application.Modules.Caja.Ports.In;
using TestDeIa.Application.Modules.Caja.UseCases;
using TestDeIa.Application.Modules.ActivosFijos.Ports.In;
using TestDeIa.Application.Modules.ActivosFijos.UseCases;
using TestDeIa.Application.Modules.Clientes.Ports.In;
using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Application.Modules.Clientes.UseCases;
using TestDeIa.Application.Modules.Contabilidad.UseCases;
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
using TestDeIa.Application.Modules.Inventario.Services;
using TestDeIa.Application.Modules.Inventario.UseCases;
using TestDeIa.Application.Modules.Personas.Ports.In;
using TestDeIa.Application.Modules.Personas.UseCases;
using TestDeIa.Application.Modules.Reporteria.Ports.In;
using TestDeIa.Application.Modules.Reporteria.UseCases;
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
        services.AddScoped<ActivoFijoService>();
        services.AddScoped<IActivoFijoUseCase, ActivoFijoUseCase>();
        services.AddScoped<IClienteUseCase, ClienteUseCase>();
        services.AddScoped<IContabilidadService, ContabilidadService>();
        services.AddScoped<ReportesFinancierosService>();
        services.AddScoped<AjusteInventarioContableService>();
        services.AddScoped<CerrarPeriodoFiscalCommandHandler>();
        services.AddScoped<IContabilidadUseCase, ContabilidadUseCase>();
        services.AddScoped<ContabilizarCompraService>();
        services.AddScoped<CuentaPorPagarService>();
        services.AddScoped<ICompraUseCase, CompraUseCase>();
        services.AddScoped<IEstudioMercadoUseCase, EstudioMercadoUseCase>();
        services.AddScoped<IProveedorUseCase, ProveedorUseCase>();
        services.AddScoped<ICompraBackgroundCoordinator, CompraBackgroundCoordinator>();
        services.AddScoped<IDashboardUseCase, DashboardUseCase>();
        services.AddScoped<IEmpleadoUseCase, EmpleadoUseCase>();
        services.AddScoped<IEmpresaUseCase, EmpresaUseCase>();
        services.AddScoped<ICertificadoDigitalEmpresaUseCase, CertificadoDigitalEmpresaUseCase>();
        services.AddScoped<IAnalizadorFiscalIAService, AnalizadorFiscalIAService>();
        services.AddScoped<IFinancieroReportesUseCase, FinancieroReportesUseCase>();
        services.AddScoped<IPersonaUseCase, PersonaUseCase>();
        services.AddScoped<IReporteVentasUseCase, ReporteVentasUseCase>();
        services.AddScoped<IInventarioService, InventarioService>();
        services.AddScoped<IInventarioUseCase, InventarioUseCase>();
        services.AddScoped<IFacturacionUseCase, FacturacionUseCase>();
        services.AddScoped<IComisionesUseCase, ComisionesUseCase>();
        services.AddScoped<IFacturacionBackgroundCoordinator, FacturacionBackgroundCoordinator>();

        return services;
    }
}


