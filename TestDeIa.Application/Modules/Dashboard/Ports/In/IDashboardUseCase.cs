using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Application.Modules.Dashboard.Ports.In;

public interface IDashboardUseCase
{
    Task<DashboardOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);
}
