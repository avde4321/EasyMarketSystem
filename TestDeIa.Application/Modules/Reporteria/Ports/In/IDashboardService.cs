using TestDeIa.Shared.Responses.Reporteria;

namespace TestDeIa.Application.Modules.Reporteria.Ports.In;

public interface IDashboardService
{
    Task<DashboardVentasDiariasResponse> ObtenerResumenVentasDiariasAsync(Guid empresaId, DateTime fecha, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TopProductoRentabilidadResponse>> ObtenerTopProductosMasVendidosAsync(Guid empresaId, int top, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<VentaBodegaMetodoPagoResponse>> ObtenerVentasPorBodegaYMetodoPagoAsync(Guid empresaId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
    Task<KpisRentabilidadResponse> ObtenerKpisRentabilidadAsync(Guid empresaId, DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
}
