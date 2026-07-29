using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Application.Modules.Dashboard.Ports.In;

public interface IDashboardUseCase
{
    Task<DashboardOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<DashboardCajeroOverviewResponse> GetCajeroOverviewAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<DashboardBodegueroOverviewResponse> GetBodegueroOverviewAsync(CancellationToken cancellationToken = default);
}
