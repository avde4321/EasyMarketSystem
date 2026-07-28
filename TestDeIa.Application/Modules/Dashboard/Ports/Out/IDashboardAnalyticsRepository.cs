using TestDeIa.Domain.Modules.Dashboard.Entities;

namespace TestDeIa.Application.Modules.Dashboard.Ports.Out;

public interface IDashboardAnalyticsRepository
{
    Task<DashboardResumenFinanciero> GetResumenMensualAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DashboardTopProducto>> GetTopProductosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DashboardTopProducto>> GetTopServiciosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductoBodegaConsumoHistorico>> GetConsumoHistoricoAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int topProductos, CancellationToken cancellationToken = default);
}