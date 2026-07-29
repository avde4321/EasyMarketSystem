using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using TestDeIa.Client.Services;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Client.Pages;

public partial class ComprobantesMonitor : IAsyncDisposable
{
    [Inject]
    private FacturacionApiClient FacturacionApiClient { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private PopupNotificationService PopupNotificationService { get; set; } = default!;

    private readonly List<FacturaMonitorResponse> facturas = [];
    private bool isLoading = true;
    private string? errorMessage;
    private PeriodicTimer? timer;
    private CancellationTokenSource? cancellationTokenSource;
    private string searchTerm = string.Empty;
    private Guid? busyFacturaId;
    private string? busyDocumentKind;
    private string? downloadStatusMessage;
    private string tipoDocumentoFiltro = string.Empty;
    private bool showNotaCreditoModal;
    private bool isLoadingNotaCreditoOrigen;
    private bool isGeneratingNotaCredito;
    private NotaCreditoOrigenResponseDto? notaCreditoOrigen;
    private string notaCreditoMotivo = string.Empty;
    private readonly Dictionary<Guid, decimal> notaCreditoCantidades = [];
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<FacturaMonitorResponse> VisibleFacturas => facturas;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        cancellationTokenSource = new CancellationTokenSource();
        timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        await LoadMonitorAsync(resetPaging: true);
        _ = PollAsync(cancellationTokenSource.Token);
    }

    private async Task LoadMonitorAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await FacturacionApiClient.GetMonitorAsync(searchTerm, tipoDocumentoFiltro, currentSkip, PageSize);
            facturas.Clear();
            facturas.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el monitor de comprobantes.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
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
            () => FacturacionApiClient.GetComprobanteRidePdfAsync(factura.Id),
            $"RIDE-{factura.TipoDocumentoId}-{factura.NumeroComprobante}.pdf",
            "application/pdf");
    }

    private async Task OpenNotaCreditoAsync(FacturaMonitorResponse factura)
    {
        if (!CanCreateNotaCredito(factura))
        {
            await PopupNotificationService.ShowInfoAsync("La nota de credito solo aplica para facturas autorizadas.");
            return;
        }

        showNotaCreditoModal = true;
        isLoadingNotaCreditoOrigen = true;
        notaCreditoOrigen = null;
        notaCreditoMotivo = string.Empty;
        notaCreditoCantidades.Clear();
        await InvokeAsync(StateHasChanged);

        var result = await FacturacionApiClient.GetNotaCreditoOrigenAsync(factura.Id);
        if (!result.Succeeded || result.Data is null)
        {
            showNotaCreditoModal = false;
            await PopupNotificationService.ShowErrorAsync(result.ErrorMessage ?? "No se pudo cargar la factura para nota de credito.");
        }
        else
        {
            notaCreditoOrigen = result.Data;
            foreach (var detalle in notaCreditoOrigen.Detalles.Where(current => current.CantidadDisponible > 0m))
            {
                notaCreditoCantidades[detalle.FacturaDetalleId] = 0m;
            }
        }

        isLoadingNotaCreditoOrigen = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task CrearNotaCreditoAsync()
    {
        if (isGeneratingNotaCredito)
        {
            return;
        }

        if (notaCreditoOrigen is null)
        {
            await PopupNotificationService.ShowErrorAsync("No se encontro la factura origen para la nota de credito.");
            return;
        }

        if (string.IsNullOrWhiteSpace(notaCreditoMotivo))
        {
            await PopupNotificationService.ShowInfoAsync("Ingresa el motivo de modificacion antes de generar la nota de credito.");
            return;
        }

        var detallesSeleccionados = notaCreditoCantidades
            .Where(current => current.Value > 0m)
            .Select(current => new NotaCreditoDetalleRequestDto
            {
                FacturaDetalleId = current.Key,
                Cantidad = current.Value
            })
            .ToArray();

        if (detallesSeleccionados.Length == 0)
        {
            await PopupNotificationService.ShowInfoAsync("Ingresa al menos una cantidad a devolver para generar la nota de credito.");
            return;
        }

        isGeneratingNotaCredito = true;
        busyFacturaId = notaCreditoOrigen.FacturaId;
        busyDocumentKind = "nota_credito";
        await PopupNotificationService.ShowInfoAsync("Generando nota de credito y registrando devolucion de inventario...");
        await InvokeAsync(StateHasChanged);

        var request = new NotaCreditoRequestDto
        {
            FacturaId = notaCreditoOrigen.FacturaId,
            MotivoModificacion = notaCreditoMotivo.Trim(),
            Detalles = detallesSeleccionados
        };

        try
        {
            var result = await FacturacionApiClient.CrearNotaCreditoAsync(request);

            if (!result.Succeeded || result.Data is null)
            {
                await PopupNotificationService.ShowErrorAsync(result.ErrorMessage ?? "No se pudo generar la nota de credito.");
                return;
            }

            showNotaCreditoModal = false;
            await PopupNotificationService.ShowSuccessAsync($"Nota de credito {result.Data.NumeroComprobante} generada y enviada a cola SRI.");
            await LoadMonitorAsync();
        }
        catch (HttpRequestException)
        {
            await PopupNotificationService.ShowErrorAsync("No se pudo comunicar con el servidor para generar la nota de credito.");
        }
        finally
        {
            isGeneratingNotaCredito = false;
            busyFacturaId = null;
            busyDocumentKind = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void CloseNotaCreditoModal()
    {
        if (isGeneratingNotaCredito)
        {
            return;
        }

        showNotaCreditoModal = false;
        notaCreditoOrigen = null;
        notaCreditoMotivo = string.Empty;
        notaCreditoCantidades.Clear();
    }

    private async Task DownloadXmlGeneradoAsync(FacturaMonitorResponse factura)
    {
        await ExecuteDownloadAsync(
            factura,
            "xml_generado",
            () => FacturacionApiClient.GetComprobanteXmlGeneradoAsync(factura.Id),
            $"{factura.TipoDocumentoId}-{factura.NumeroComprobante}-xml-generado.xml",
            "application/xml");
    }

    private async Task DownloadXmlFirmadoAsync(FacturaMonitorResponse factura)
    {
        await ExecuteDownloadAsync(
            factura,
            "xml_firmado",
            () => FacturacionApiClient.GetComprobanteXmlFirmadoAsync(factura.Id),
            $"{factura.TipoDocumentoId}-{factura.NumeroComprobante}-xml-firmado.xml",
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
        await PopupNotificationService.ShowInfoAsync(downloadStatusMessage);
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await action();

            if (!result.Succeeded || result.FileBytes is null)
            {
                errorMessage = result.ErrorMessage ?? $"No se pudo generar {GetFriendlyDocumentName(documentKind)}.";
                downloadStatusMessage = null;
                await PopupNotificationService.ShowErrorAsync(errorMessage);
                return;
            }

            await DownloadAsync(fileName, contentType, result.FileBytes);
            downloadStatusMessage = $"{GetFriendlyDocumentName(documentKind)} generado correctamente.";
            await PopupNotificationService.ShowSuccessAsync(downloadStatusMessage);
        }
        catch (Exception)
        {
            errorMessage = $"Ocurrio un error al generar {GetFriendlyDocumentName(documentKind)}.";
            downloadStatusMessage = null;
            await PopupNotificationService.ShowErrorAsync(errorMessage);
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

    private static bool CanCreateNotaCredito(FacturaMonitorResponse factura)
    {
        return factura.TipoDocumentoId == "01" && factura.Estado.Equals("Autorizado", StringComparison.OrdinalIgnoreCase);
    }

    private decimal GetNotaCreditoCantidad(Guid detalleId)
    {
        return notaCreditoCantidades.TryGetValue(detalleId, out var cantidad) ? cantidad : 0m;
    }

    private void SetNotaCreditoCantidad(Guid detalleId, decimal value)
    {
        var max = notaCreditoOrigen?.Detalles.FirstOrDefault(current => current.FacturaDetalleId == detalleId)?.CantidadDisponible ?? 0m;
        notaCreditoCantidades[detalleId] = Math.Clamp(value, 0m, max);
    }

    private void OnNotaCreditoCantidadChanged(Guid detalleId, ChangeEventArgs eventArgs)
    {
        var rawValue = Convert.ToString(eventArgs.Value, CultureInfo.InvariantCulture);
        if (!decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            value = 0m;
        }

        SetNotaCreditoCantidad(detalleId, value);
    }

    private bool CanSubmitNotaCredito =>
        notaCreditoOrigen is not null &&
        !isGeneratingNotaCredito &&
        !string.IsNullOrWhiteSpace(notaCreditoMotivo) &&
        notaCreditoCantidades.Any(current => current.Value > 0m);

    private Task SearchAsync() => LoadMonitorAsync(resetPaging: true);

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadMonitorAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadMonitorAsync();
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
