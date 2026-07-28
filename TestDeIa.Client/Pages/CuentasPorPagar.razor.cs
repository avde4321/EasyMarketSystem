using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Client.Services.Contabilidad;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Responses.Contabilidad;

namespace TestDeIa.Client.Pages;

public partial class CuentasPorPagar
{
    [Inject]
    private CuentasPorPagarApiClient CuentasPorPagarApiClient { get; set; } = default!;

    [Inject]
    private ProveedoresApiClient ProveedoresApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private ContabilidadApiClient ContabilidadApiClient { get; set; } = default!;

    [Inject]
    private PopupNotificationService PopupNotificationService { get; set; } = default!;

    private readonly List<CuentaPorPagarResponse> cuentas = [];
    private readonly List<ProveedorResponse> proveedores = [];
    private readonly List<CatalogoItemResponse> formasPago = [];
    private readonly List<CuentaContableResponse> cuentasMonetarias = [];
    private CuentasPorPagarResumenResponse resumen = new();
    private RegistrarAbonoCxPRequest abonoRequest = new();
    private CuentaPorPagarResponse? selectedCuenta;
    private string searchTerm = string.Empty;
    private string selectedProveedorIdString = string.Empty;
    private bool isLoading = true;
    private bool isSaving;
    private bool isAbonoModalOpen;
    private string? errorMessage;
    private string? successMessage;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private DateTime fechaPagoLocal = DateTime.Today;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await LoadLookupsAsync();
        await LoadResumenAsync();
        await LoadCuentasAsync(resetPaging: true);
    }

    private async Task LoadLookupsAsync()
    {
        var proveedoresPage = await ProveedoresApiClient.GetPagedAsync(null, 0, 200);
        proveedores.Clear();
        proveedores.AddRange(proveedoresPage.Items.Where(current => current.IsActive));

        formasPago.Clear();
        formasPago.AddRange(await CatalogosApiClient.GetItemsAsync("FORMA_PAGO_SRI", true));

        cuentasMonetarias.Clear();
        cuentasMonetarias.AddRange((await ContabilidadApiClient.GetCuentasAceptablesAsync())
            .Where(current =>
                current.Codigo.StartsWith("1.1.01", StringComparison.OrdinalIgnoreCase) ||
                current.Nombre.Contains("Caja", StringComparison.OrdinalIgnoreCase) ||
                current.Nombre.Contains("Banco", StringComparison.OrdinalIgnoreCase))
            .OrderBy(current => current.Codigo));
    }

    private async Task LoadResumenAsync()
    {
        resumen = await CuentasPorPagarApiClient.GetResumenAsync();
    }

    private async Task LoadCuentasAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var proveedorId = Guid.TryParse(selectedProveedorIdString, out var parsedProveedorId) ? parsedProveedorId : (Guid?)null;
            var page = await CuentasPorPagarApiClient.GetPagedAsync(searchTerm, proveedorId, currentSkip, PageSize);
            cuentas.Clear();
            cuentas.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el monitor de cuentas por pagar.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task SearchAsync()
    {
        await LoadResumenAsync();
        await LoadCuentasAsync(resetPaging: true);
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadCuentasAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadCuentasAsync();
    }

    private void OpenAbonoModal(CuentaPorPagarResponse cuenta)
    {
        selectedCuenta = cuenta;
        fechaPagoLocal = DateTime.Today;
        abonoRequest = new RegistrarAbonoCxPRequest
        {
            CuentaPorPagarId = cuenta.Id,
            FormaPago = formasPago.FirstOrDefault(current => current.Codigo == "20")?.Codigo ?? formasPago.FirstOrDefault()?.Codigo ?? "01",
            MontoPagado = cuenta.SaldoActual,
            CuentaContableSalidaId = cuentasMonetarias.FirstOrDefault()?.Id ?? Guid.Empty
        };
        errorMessage = null;
        successMessage = null;
        isAbonoModalOpen = true;
    }

    private void CloseAbonoModal()
    {
        isAbonoModalOpen = false;
        selectedCuenta = null;
        isSaving = false;
    }

    private async Task SaveAbonoAsync()
    {
        if (selectedCuenta is null)
        {
            return;
        }

        if (abonoRequest.MontoPagado <= 0)
        {
            errorMessage = "El abono debe ser mayor a cero.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
            return;
        }

        if (abonoRequest.MontoPagado > selectedCuenta.SaldoActual)
        {
            errorMessage = "El abono no puede superar el saldo disponible.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
            return;
        }

        if (abonoRequest.CuentaContableSalidaId == Guid.Empty)
        {
            errorMessage = "Selecciona la cuenta monetaria de salida.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
            return;
        }

        isSaving = true;
        errorMessage = null;
        successMessage = null;
        abonoRequest.FechaPago = new DateTimeOffset(fechaPagoLocal.Year, fechaPagoLocal.Month, fechaPagoLocal.Day, 0, 0, 0, DateTimeOffset.Now.Offset);

        try
        {
            var result = await CuentasPorPagarApiClient.RegistrarAbonoAsync(abonoRequest);
            if (!result.Succeeded || result.Data is null)
            {
                errorMessage = result.ErrorMessage ?? "No se pudo registrar el abono.";
                await PopupNotificationService.ShowErrorAsync(errorMessage);
                return;
            }

            successMessage = $"Abono aplicado. Nuevo saldo: {result.Data.SaldoActual:0.00}.";
            await PopupNotificationService.ShowSuccessAsync(successMessage);
            CloseAbonoModal();
            await LoadResumenAsync();
            await LoadCuentasAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo registrar el abono.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isSaving = false;
        }
    }
}
