using TestDeIa.Domain.Modules.Dashboard.Entities;

namespace TestDeIa.Application.Modules.Dashboard.Ports.Out;

public interface IInventarioPredictivoService
{
    Task<IReadOnlyCollection<DashboardAlertaPredictivaStock>> PredecirAlertasAsync(
        IReadOnlyCollection<ProductoBodegaConsumoHistorico> historial,
        CancellationToken cancellationToken = default);
}
