using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Client.Pages;

public partial class ComprobantesMonitor : IAsyncDisposable
{
    [Inject]
    private FacturacionApiClient FacturacionApiClient { get; set; } = default!;

    private readonly List<FacturaMonitorResponse> facturas = [];
    private bool isLoading = true;
    private string? errorMessage;
    private PeriodicTimer? timer;
    private CancellationTokenSource? cancellationTokenSource;

    protected override async Task OnInitializedAsync()
    {
        cancellationTokenSource = new CancellationTokenSource();
        timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        await LoadMonitorAsync();
        _ = PollAsync(cancellationTokenSource.Token);
    }

    private async Task LoadMonitorAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            facturas.Clear();
            facturas.AddRange(await FacturacionApiClient.GetMonitorAsync());
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el monitor de comprobantes.";
        }
        finally
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        if (timer is null)
        {
            return;
        }

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await LoadMonitorAsync();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (cancellationTokenSource is not null)
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
        }

        timer?.Dispose();
    }
}
