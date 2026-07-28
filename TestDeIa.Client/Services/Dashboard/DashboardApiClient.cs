using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Client.Services.Dashboard;

public sealed class DashboardApiClient(HttpClient httpClient)
{
    public async Task<DashboardOverviewResponse> GetOverviewAsync()
    {
        return await httpClient.GetFromJsonAsync<DashboardOverviewResponse>("api/dashboard/overview")
               ?? new DashboardOverviewResponse();
    }
}
