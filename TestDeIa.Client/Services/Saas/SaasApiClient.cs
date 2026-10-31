using System.Net.Http.Json;
using TestDeIa.Shared.Responses.Saas;

namespace TestDeIa.Client.Services.Saas;

public sealed class SaasApiClient(HttpClient httpClient)
{
    public async Task<SubscriptionStatusResponse> GetSubscriptionStatusAsync()
        => await httpClient.GetFromJsonAsync<SubscriptionStatusResponse>("api/saas/subscription-status")
            ?? new SubscriptionStatusResponse(false, "SUBSCRIPTION_REQUIRED");
    public async Task<IReadOnlyCollection<SaasPlanResponse>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<SaasPlanResponse>>("api/saas/plans", cancellationToken)
            ?? [];
    }
}
