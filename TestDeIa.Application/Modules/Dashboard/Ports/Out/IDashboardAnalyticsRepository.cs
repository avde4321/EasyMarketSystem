using TestDeIa.Domain.Modules.Dashboard.Entities;
using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Application.Modules.Dashboard.Ports.Out;

public interface IDashboardAnalyticsRepository
{
    Task<DashboardResumenFinanciero> GetResumenMensualAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DashboardTopProducto>> GetTopProductosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DashboardTopProducto>> GetTopServiciosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductoBodegaConsumoHistorico>> GetConsumoHistoricoAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int topProductos, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalVentasAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DashboardProductividadUsuarioResponse>> GetProductividadUsuariosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default);
    Task<DashboardCajeroOverviewResponse> GetCajeroOverviewAsync(Guid usuarioId, DateTimeOffset periodoInicio, DateTimeOffset periodoFin, CancellationToken cancellationToken = default);
    Task<DashboardBodegueroOverviewResponse> GetBodegueroOverviewAsync(DateTimeOffset diaInicio, DateTimeOffset diaFin, DateTimeOffset mesInicio, DateTimeOffset mesFin, CancellationToken cancellationToken = default);
}
