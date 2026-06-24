using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Client.Pages;

public partial class ComprobantesMonitor : IAsyncDisposable
{
    [Inject]
    private FacturacionApiClient FacturacionApiClient { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private readonly List<FacturaMonitorResponse> facturas = [];
    private bool isLoading = true;
    private string? errorMessage;
    private PeriodicTimer? timer;
    private CancellationTokenSource? cancellationTokenSource;
    private string searchTerm = string.Empty;
    private Guid? busyFacturaId;
    private string? busyDocumentKind;
    private string? downloadStatusMessage;

    private IEnumerable<FacturaMonitorResponse> FilteredFacturas => facturas.Where(factura =>
        string.IsNullOrWhiteSpace(searchTerm) ||
        factura.NumeroComprobante.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        factura.ClienteNombre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        factura.ClienteIdentificacion.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        factura.ClienteTipoIdentificacion.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        factura.FormaPago.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        factura.Estado.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
        (factura.MensajeEstado?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

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

    private async Task DownloadRideAsync(FacturaMonitorResponse factura)
    {
        await ExecuteDownloadAsync(
            factura,
            "ride",
            () => FacturacionApiClient.GetRidePdfAsync(factura.Id),
            $"RIDE-{factura.NumeroComprobante}.pdf",
            "application/pdf");
    }

    private async Task DownloadXmlGeneradoAsync(FacturaMonitorResponse factura)
    {
        await ExecuteDownloadAsync(
            factura,
            "xml_generado",
            () => FacturacionApiClient.GetXmlGeneradoAsync(factura.Id),
            $"FACTURA-{factura.NumeroComprobante}-xml-generado.xml",
            "application/xml");
    }

    private async Task DownloadXmlFirmadoAsync(FacturaMonitorResponse factura)
    {
        await ExecuteDownloadAsync(
            factura,
            "xml_firmado",
            () => FacturacionApiClient.GetXmlFirmadoAsync(factura.Id),
            $"FACTURA-{factura.NumeroComprobante}-xml-firmado.xml",
            "application/xml");
    }

    private ValueTask DownloadAsync(string fileName, string contentType, byte[] bytes)
    {
        return JsRuntime.InvokeVoidAsync("easyMarketDownload.saveFileFromBytes", fileName, contentType, Convert.ToBase64String(bytes));
    }

    private async Task ExecuteDownloadAsync(
        FacturaMonitorResponse factura,
        string documentKind,
        Func<Task<(bool Succeeded, string? ErrorMessage, byte[]? FileBytes)>> action,
        string fileName,
        string contentType)
    {
        if (IsBusyFor(factura.Id))
        {
            return;
        }

        busyFacturaId = factura.Id;
        busyDocumentKind = documentKind;
        errorMessage = null;
        downloadStatusMessage = $"Generando {GetFriendlyDocumentName(documentKind)} de {factura.NumeroComprobante}...";
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await action();

            if (!result.Succeeded || result.FileBytes is null)
            {
                errorMessage = result.ErrorMessage ?? $"No se pudo generar {GetFriendlyDocumentName(documentKind)}.";
                downloadStatusMessage = null;
                return;
            }

            await DownloadAsync(fileName, contentType, result.FileBytes);
            downloadStatusMessage = $"{GetFriendlyDocumentName(documentKind)} generado correctamente.";
        }
        catch (Exception)
        {
            errorMessage = $"Ocurrio un error al generar {GetFriendlyDocumentName(documentKind)}.";
            downloadStatusMessage = null;
        }
        finally
        {
            busyFacturaId = null;
            busyDocumentKind = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool IsBusyFor(Guid facturaId)
    {
        return busyFacturaId.HasValue && busyFacturaId.Value == facturaId;
    }

    private string GetRideLabel(Guid facturaId)
    {
        return IsBusyFor(facturaId) && busyDocumentKind == "ride" ? "Generando..." : "RIDE";
    }

    private string GetXmlGeneradoLabel(Guid facturaId)
    {
        return IsBusyFor(facturaId) && busyDocumentKind == "xml_generado" ? "Generando..." : "XML Base";
    }

    private string GetXmlFirmadoLabel(Guid facturaId)
    {
        return IsBusyFor(facturaId) && busyDocumentKind == "xml_firmado" ? "Generando..." : "XML Firmado";
    }

    private static string GetFriendlyDocumentName(string documentKind)
    {
        return documentKind switch
        {
            "ride" => "el RIDE",
            "xml_generado" => "el XML base",
            "xml_firmado" => "el XML firmado",
            _ => "el documento"
        };
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
