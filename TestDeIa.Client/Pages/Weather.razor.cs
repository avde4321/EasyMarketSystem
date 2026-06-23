using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace TestDeIa.Client.Pages;

public partial class Weather
{
    [Inject]
    private HttpClient Http { get; set; } = default!;

    private HealthStatus? healthStatus;

    protected override async Task OnInitializedAsync()
    {
        healthStatus = await Http.GetFromJsonAsync<HealthStatus>("api/health");
    }

    private sealed class HealthStatus
    {
        public string Status { get; set; } = string.Empty;
    }
}
