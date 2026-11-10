using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TestDeIa.Client.Services.Caja;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Client.Services.OfflinePos;
using TestDeIa.Client.Services;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Requests.OfflinePos;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Caja;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Responses.OfflinePos;
using TestDeIa.Shared.Security;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Client.Pages;

public partial class FacturacionPos : IDisposable
{
    [Inject]
    private FacturacionApiClient FacturacionApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private ClientesApiClient ClientesApiClient { get; set; } = default!;

    [Inject]
    private CajaApiClient CajaApiClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private PopupNotificationService PopupNotificationService { get; set; } = default!;

    [Inject]
    private PosOfflineApiClient PosOfflineApiClient { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private readonly List<PosClienteResponse> clienteResults = [];
    private readonly List<PosProductoResponse> productoResults = [];
    private readonly List<PosPuntoEmisionResponse> puntosEmision = [];
    private readonly List<PosOperadorResponse> operadores = [];
    private Guid? administradorOperadorPredeterminado;
    private string? operadoresErrorMessage;
    private readonly List<CatalogoItemResponse> formasPago = [];
    private readonly List<CartItemModel> cartItems = [];
    private string clienteSearchTerm = string.Empty;
    private string productoSearchTerm = string.Empty;
    private PosClienteResponse? selectedCliente;
    private PosPuntoEmisionResponse? selectedPuntoEmision;
    private string formaPago = "01";
    private string? observacion;
    private decimal? efectivoRecibido;
    private bool isSubmitting;
    private bool isSearchingProducts;
    private bool showOperationalContextModal;
    private bool isCreatingClienteExtension;
    private string? errorMessage;
    private string? statusMessage;
    private string? posInfoModalTitle;
    private string? posInfoModalMessage;
    private bool showPosInfoModal;
    private bool isCajaLoading;
    private bool hasCajaActiva;
    private bool hasCajaVencida;
    private CajaSesionResponse? cajaActiva;
    private bool showCajaRequiredModal;
    private bool canEditServicePrice;
    private bool isOnline = true;
    private bool isSyncingOfflineQueue;
    private bool showWhatsAppModal;
    private bool showPagoQrModal;
    private FacturaEmissionResponse? lastFactura;
    private decimal lastFacturaTotal;
    private DotNetObjectReference<FacturacionPos>? dotNetReference;
    private IJSObjectReference? networkHandlerReference;
    private ElementReference productSearchInput;
    private ElementReference paymentAmountInput;
    private ElementReference checkoutButton;
    private const int SearchPageSize = 8;
    private int clienteSkip;
    private int productoSkip;
    private int clienteTotalCount;
    private int productoTotalCount;
    private bool CanGoPreviousClientes => clienteSkip > 0;
    private bool CanGoNextClientes => clienteSkip + SearchPageSize < clienteTotalCount;
    private bool CanGoPreviousProductos => productoSkip > 0;
    private bool CanGoNextProductos => productoSkip + SearchPageSize < productoTotalCount;
    private bool HasOperationalContext => selectedPuntoEmision is not null;
    private bool CanOperatePos => HasOperationalContext && hasCajaActiva && !hasCajaVencida;
    private bool CanOperateSearch => HasOperationalContext && ((hasCajaActiva && !hasCajaVencida) || !isOnline);
    private bool IsCashPayment => string.Equals(SriCatalogCodes.NormalizeFormaPagoCode(formaPago), SriCatalogCodes.FormaPagoEfectivo, StringComparison.Ordinal);
    private decimal VueltoCalculado => Math.Max(0m, Math.Round((efectivoRecibido ?? 0m) - GetCartTotal(), 2, MidpointRounding.AwayFromZero));
    private decimal MontoPendienteEfectivo => IsCashPayment ? Math.Max(0m, Math.Round(GetCartTotal() - (efectivoRecibido ?? 0m), 2, MidpointRounding.AwayFromZero)) : 0m;
    private bool IsCashPaymentCovered => !IsCashPayment || GetCartTotal() <= 0 || (efectivoRecibido ?? 0m) >= GetCartTotal();
    private string CajaModalTitle => hasCajaVencida ? "Caja pendiente de cierre" : "Caja cerrada";
    private string CajaModalMessage => hasCajaVencida && cajaActiva is not null
        ? $"Tienes una caja abierta desde {cajaActiva.FechaApertura.ToLocalTime():dd/MM/yyyy HH:mm}. Debes cerrarla antes de operar el POS y abrir una nueva caja con la fecha correcta de hoy."
        : "No tienes una caja abierta para operar el POS. Debes abrir la caja antes de buscar clientes, cargar productos o facturar.";
    private string CajaModalActionText => hasCajaVencida ? "Ir a cerrar caja" : "Ir a abrir caja";
    private string LastFacturaRideLink => lastFactura is null ? string.Empty : $"api/reporteria/facturas/{lastFactura.FacturaId}/ride";
    private string LastFacturaXmlLink => lastFactura is null ? string.Empty : $"api/reporteria/facturas/{lastFactura.FacturaId}/xml-generado";

    protected override async Task OnInitializedAsync()
    {
        await ResolveCurrentUserCapabilitiesAsync();
        await LoadCatalogosAsync();
        await LoadPuntosEmisionAsync();
        await LoadOperadoresAsync();
        await LoadCajaStateAsync();
        await InitializeOfflineModeAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !showCajaRequiredModal && !showOperationalContextModal && CanOperateSearch)
        {
            await FocusProductSearchAsync();
        }
    }

    private async Task FocusProductSearchAsync()
    {
        await productSearchInput.FocusAsync(preventScroll: true);
    }

    private async Task FocusPaymentAsync()
    {
        if (IsCashPayment)
        {
            await paymentAmountInput.FocusAsync(preventScroll: true);
            return;
        }

        await checkoutButton.FocusAsync();
    }

    private async Task HandlePosKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "F2":
                if (!IsPosModalOpen && CanOperateSearch)
                {
                    await FocusProductSearchAsync();
                }
                break;
            case "F4":
                if (!IsPosModalOpen && CanOperatePos && cartItems.Count > 0)
                {
                    await FocusPaymentAsync();
                }
                break;
            case "Escape":
                CloseTopmostModalOrClearSearch();
                break;
        }
    }

    private async Task HandlePaymentKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" && !isSubmitting)
        {
            await CobrarAsync();
        }
    }

    private bool IsPosModalOpen => showCajaRequiredModal || showOperationalContextModal || showPosInfoModal || showWhatsAppModal || showPagoQrModal;

    private void CloseTopmostModalOrClearSearch()
    {
        if (showPagoQrModal) ClosePagoQrModal();
        else if (showWhatsAppModal) CloseWhatsAppModal();
        else if (showPosInfoModal) ClosePosInfoModal();
        else if (showOperationalContextModal) showOperationalContextModal = false;
        else if (showCajaRequiredModal) showCajaRequiredModal = false;
        else
        {
            productoSearchTerm = string.Empty;
            productoResults.Clear();
            productoTotalCount = 0;
        }
    }

    private async Task ResolveCurrentUserCapabilitiesAsync()
    {
        if (AuthenticationStateTask is null)
        {
            canEditServicePrice = false;
            return;
        }

        var authenticationState = await AuthenticationStateTask;
        var user = authenticationState.User;
        canEditServicePrice = user.Identity?.IsAuthenticated == true &&
            (user.IsInRole(SecurityRoleNames.SuperAdministrador) ||
             user.IsInRole(SecurityRoleNames.Administrador) ||
             user.IsInRole(SecurityRoleNames.AdministradorEmpresa));
        administradorOperadorPredeterminado = canEditServicePrice &&
            Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null;
    }

    private async Task LoadCatalogosAsync()
    {
        formasPago.Clear();
        formasPago.AddRange(await CatalogosApiClient.GetItemsAsync("FORMA_PAGO_SRI", true));
        formaPago = formasPago.FirstOrDefault()?.Codigo ?? "01";
    }

    private async Task LoadPuntosEmisionAsync()
    {
        puntosEmision.Clear();
        puntosEmision.AddRange(await FacturacionApiClient.GetPuntosEmisionAsync());

        if (puntosEmision.Count == 0)
        {
            errorMessage = "La empresa activa no tiene puntos de emision configurados. Debes parametrizarlos antes de facturar.";
            statusMessage = null;
            showOperationalContextModal = false;
            selectedPuntoEmision = null;
            return;
        }

        selectedPuntoEmision ??= puntosEmision.FirstOrDefault(current => current.IsDefault) ?? puntosEmision[0];
        showOperationalContextModal = !showCajaRequiredModal;
    }

    private async Task LoadOperadoresAsync()
    {
        operadores.Clear();
        operadoresErrorMessage = null;

        try
        {
            operadores.AddRange(await FacturacionApiClient.GetOperadoresAsync());
        }
        catch (HttpRequestException)
        {
            operadoresErrorMessage = "No se pudo cargar la lista de operadores. Reintenta la consulta antes de facturar un servicio.";
        }
    }

    private Task RetryLoadOperadoresAsync() => LoadOperadoresAsync();

    private async Task LoadCajaStateAsync()
    {
        isCajaLoading = true;

        try
        {
            cajaActiva = await CajaApiClient.GetActivaAsync();
            hasCajaActiva = cajaActiva is not null;
            hasCajaVencida = cajaActiva is not null && cajaActiva.FechaApertura.ToLocalTime().Date < DateTime.Today;
            showCajaRequiredModal = !hasCajaActiva || hasCajaVencida;
            if (showCajaRequiredModal)
            {
                showOperationalContextModal = false;
                errorMessage = hasCajaVencida
                    ? "Tienes una caja abierta de un dia anterior. Cierra esa caja antes de volver a operar el POS."
                    : "Debes abrir una caja para este usuario antes de operar el POS.";
            }
        }
        catch (HttpRequestException)
        {
            hasCajaActiva = false;
            hasCajaVencida = false;
            cajaActiva = null;
            showCajaRequiredModal = true;
        }
        finally
        {
            isCajaLoading = false;
        }
    }

    private async Task SearchClientesAsync()
    {
        clienteSkip = 0;
        await LoadClientesPageAsync();
    }

    private async Task LoadClientesPageAsync()
    {
        errorMessage = null;
        statusMessage = null;
        clienteResults.Clear();

        if (!EnsureOperationalContextForSearch())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(clienteSearchTerm) || clienteSearchTerm.Trim().Length < 2)
        {
            return;
        }

        var page = await FacturacionApiClient.SearchClientesAsync(clienteSearchTerm.Trim(), clienteSkip, SearchPageSize);
        clienteResults.AddRange(page.Items);
        clienteTotalCount = page.TotalCount;
    }

    private async Task SearchProductosAsync()
    {
        if (!CanOperateSearch || isSearchingProducts)
        {
            return;
        }

        productoSkip = 0;
        await LoadProductosPageAsync();
    }

    private Task SubmitProductSearchAsync()
    {
        return SearchProductosAsync();
    }

    private async Task LoadProductosPageAsync()
    {
        errorMessage = null;
        statusMessage = null;
        productoResults.Clear();

        if (!EnsureOperationalContextForSearch())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(productoSearchTerm) || productoSearchTerm.Trim().Length < 2)
        {
            return;
        }

        isSearchingProducts = true;
        try
        {
            if (!isOnline)
            {
                var cachedProducts = await JsRuntime.InvokeAsync<List<StockLocalCacheDto>>("easyMarketPosOffline.getCatalogo");
                var normalizedTerm = productoSearchTerm.Trim();
                var matchingProducts = cachedProducts
                    .Where(current =>
                        current.BodegaId == selectedPuntoEmision?.BodegaId &&
                        (current.CodigoBarra.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase) ||
                         current.Nombre.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                productoTotalCount = matchingProducts.Length;
                productoResults.AddRange(matchingProducts
                    .Skip(productoSkip)
                    .Take(SearchPageSize)
                    .Select(current => new PosProductoResponse
                    {
                        ProductoId = current.ProductoId,
                        Codigo = current.CodigoBarra,
                        Nombre = current.Nombre,
                        PrecioVenta = current.Precio,
                        PorcentajeIva = current.TarifaIVA,
                        StockActual = current.StockDisponible,
                        ControlaStock = current.ControlaStock
                    }));
                return;
            }

            var page = await FacturacionApiClient.SearchProductosAsync(productoSearchTerm.Trim(), productoSkip, SearchPageSize, selectedPuntoEmision?.BodegaId);
            productoResults.AddRange(page.Items);
            productoTotalCount = page.TotalCount;
        }
        catch (HttpRequestException exception)
        {
            errorMessage = exception.Message;
            productoTotalCount = 0;
        }
        catch (JSException)
        {
            errorMessage = "No se pudo consultar el catálogo local. Actualiza la caché del POS cuando tengas conexión.";
            productoTotalCount = 0;
        }
        finally
        {
            isSearchingProducts = false;
        }
    }

    private void SelectCliente(PosClienteResponse cliente)
    {
        selectedCliente = cliente;
        clienteResults.Clear();
        clienteSearchTerm = cliente.Identificacion;
    }

    private async Task PreviousClientesAsync()
    {
        if (!CanGoPreviousClientes)
        {
            return;
        }

        clienteSkip = Math.Max(0, clienteSkip - SearchPageSize);
        await LoadClientesPageAsync();
    }

    private async Task NextClientesAsync()
    {
        if (!CanGoNextClientes)
        {
            return;
        }

        clienteSkip += SearchPageSize;
        await LoadClientesPageAsync();
    }

    private async Task PreviousProductosAsync()
    {
        if (!CanGoPreviousProductos)
        {
            return;
        }

        productoSkip = Math.Max(0, productoSkip - SearchPageSize);
        await LoadProductosPageAsync();
    }

    private async Task NextProductosAsync()
    {
        if (!CanGoNextProductos)
        {
            return;
        }

        productoSkip += SearchPageSize;
        await LoadProductosPageAsync();
    }

    private void AddProducto(PosProductoResponse producto)
    {
        errorMessage = null;
        statusMessage = null;

        if (!EnsureOperationalContext())
        {
            return;
        }

        if (producto.ControlaStock)
        {
            if (producto.StockActual <= 0)
            {
                errorMessage = $"{producto.Nombre} no tiene existencias en la bodega del punto de emisión.";
                return;
            }

            var existing = cartItems.FirstOrDefault(item => item.ProductoId == producto.ProductoId && item.ControlaStock);
            if (existing is not null)
            {
                if (existing.Cantidad + 1 > producto.StockActual)
                {
                    errorMessage = $"La cantidad de {producto.Nombre} no puede superar las {producto.StockActual:0.##} unidades disponibles en esta bodega.";
                    return;
                }

                existing.Cantidad += 1;
                statusMessage = $"Cantidad de {producto.Nombre} actualizada en la factura.";
                return;
            }
        }

        cartItems.Add(new CartItemModel
        {
            RowId = Guid.NewGuid(),
            ProductoId = producto.ProductoId,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            PrecioVenta = producto.PrecioVenta,
            PorcentajeIva = producto.PorcentajeIva,
            ControlaStock = producto.ControlaStock,
            StockDisponible = producto.StockActual,
            UsuarioIdOperador = !producto.ControlaStock && administradorOperadorPredeterminado.HasValue &&
                operadores.Any(operador => operador.UsuarioId == administradorOperadorPredeterminado.Value)
                    ? administradorOperadorPredeterminado : null,
            Cantidad = 1
        });
        statusMessage = $"{producto.Nombre} agregado a la factura.";
    }

    private void AddCashDenomination(decimal amount)
    {
        efectivoRecibido = Math.Round((efectivoRecibido ?? 0m) + amount, 2, MidpointRounding.AwayFromZero);
    }

    private void SetExactCash()
    {
        efectivoRecibido = GetCartTotal();
    }

    private void ClearCash()
    {
        efectivoRecibido = null;
    }

    private void IncrementQuantity(CartItemModel item)
    {
        if (item.ControlaStock && item.Cantidad + 1 > item.StockDisponible)
        {
            errorMessage = $"Solo hay {item.StockDisponible:0.##} unidades disponibles de {item.Nombre} en esta bodega.";
            return;
        }

        item.Cantidad += 1;
    }

    private void DecrementQuantity(CartItemModel item)
    {
        if (item.Cantidad <= 1)
        {
            return;
        }

        item.Cantidad -= 1;
    }

    private void RemoveProducto(Guid rowId)
    {
        var existing = cartItems.FirstOrDefault(item => item.RowId == rowId);
        if (existing is not null)
        {
            cartItems.Remove(existing);
        }
    }

    private async Task CobrarAsync()
    {
        if (isSubmitting)
        {
            return;
        }

        errorMessage = null;
        statusMessage = null;

        if (selectedCliente is null)
        {
            errorMessage = "Selecciona un cliente antes de cobrar.";
            ShowPosInfoModal("Datos pendientes", errorMessage);
            return;
        }

        if (!selectedCliente.HasClienteExtension || !selectedCliente.ClienteId.HasValue)
        {
            errorMessage = "La persona seleccionada aun no tiene el rol de cliente en esta empresa. Activalo antes de facturar.";
            ShowPosInfoModal("Cliente pendiente de activar", errorMessage);
            return;
        }

        if (!EnsureOperationalContext())
        {
            return;
        }

        if (cartItems.Count == 0)
        {
            errorMessage = "Agrega al menos un producto al carrito.";
            ShowPosInfoModal("Detalle pendiente", errorMessage);
            return;
        }

        if (cartItems.Any(item => item.Cantidad <= 0))
        {
            errorMessage = "Todas las cantidades deben ser mayores a cero.";
            ShowPosInfoModal("Revisa las cantidades", errorMessage);
            return;
        }

        if (cartItems.Any(item => item.IsService && (!item.UsuarioIdOperador.HasValue || item.UsuarioIdOperador.Value == Guid.Empty)))
        {
            errorMessage = "Cada servicio debe tener un operador asignado antes de cobrar.";
            ShowPosInfoModal("Operador pendiente", errorMessage);
            return;
        }

        if (cartItems.Any(item => item.IsService && item.PrecioVenta < 0))
        {
            errorMessage = "El precio de los servicios no puede ser negativo.";
            ShowPosInfoModal("Revisa el precio", errorMessage);
            return;
        }

        if (IsCashPayment && !IsCashPaymentCovered)
        {
            errorMessage = $"El efectivo recibido no cubre el total. Faltan {MontoPendienteEfectivo:0.00}.";
            ShowPosInfoModal("Efectivo insuficiente", errorMessage);
            return;
        }

        isSubmitting = true;
        statusMessage = isOnline
            ? "Registrando la factura. La firma y el envío al SRI continuarán en segundo plano."
            : "Guardando la venta localmente para sincronizarla cuando vuelva la conexión.";
        await InvokeAsync(StateHasChanged);

        try
        {
            if (!isOnline)
            {
                await SaveOfflineSaleAsync();
                return;
            }

            var result = await FacturacionApiClient.EmitirFacturaAsync(new EmitirFacturaRequest
            {
                ClienteId = selectedCliente.ClienteId.Value,
                BodegaId = selectedPuntoEmision!.BodegaId,
                Establecimiento = selectedPuntoEmision!.Establecimiento,
                PuntoEmision = selectedPuntoEmision.PuntoEmision,
                FormaPago = formaPago,
                Observacion = observacion,
                MontoRecibido = IsCashPayment ? efectivoRecibido : null,
                VueltoEntregado = IsCashPayment ? VueltoCalculado : null,
                Items = cartItems.Select(item => new EmitirFacturaDetalleRequest
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitarioOverride = item.IsService ? item.PrecioVenta : null,
                    UsuarioIdOperador = item.IsService ? item.UsuarioIdOperador : null
                }).ToArray()
            });

            if (!result.Succeeded)
            {
                ShowPosInfoModal(
                    "No se pudo emitir la factura",
                    result.ErrorMessage ?? "No se pudo registrar la factura.");
                return;
            }

            lastFacturaTotal = GetCartTotal();
            statusMessage = $"{result.Data?.NumeroComprobante} registrada en estado pendiente. La validacion SRI sigue en segundo plano.";
            lastFactura = result.Data;
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
            cartItems.Clear();
            productoResults.Clear();
            observacion = null;
            efectivoRecibido = null;
            await LoadCajaStateAsync();
        }
        catch (HttpRequestException)
        {
            await SaveOfflineSaleAsync();
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private async Task ActivarClienteExtensionAsync()
    {
        errorMessage = null;
        statusMessage = null;

        if (selectedCliente is null)
        {
            errorMessage = "Selecciona una persona antes de crear la extension de cliente.";
            return;
        }

        isCreatingClienteExtension = true;

        try
        {
            var request = new ClienteRequest
            {
                TipoIdentificacion = selectedCliente.TipoIdentificacion,
                Identificacion = selectedCliente.Identificacion,
                RazonSocialONombresCompletos = selectedCliente.NombreCompleto,
                NombreComercial = selectedCliente.NombreComercial,
                DireccionPrincipal = string.IsNullOrWhiteSpace(selectedCliente.Direccion) ? "Sin direccion registrada" : selectedCliente.Direccion,
                CorreoElectronicoPrincipal = selectedCliente.Email,
                CorreoFacturacionElectronica = selectedCliente.Email,
                TelefonoCelular = selectedCliente.Telefono,
                TipoCliente = selectedCliente.TipoIdentificacion == "04" ? "Juridico" : "Natural",
                EstadoCredito = "Normal",
                IsActive = true
            };

            var result = await ClientesApiClient.CreateAsync(request);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage ?? "No se pudo crear la extension comercial del cliente.";
                return;
            }

            statusMessage = "La persona ahora quedo habilitada como cliente para la empresa activa.";
            await PopupNotificationService.ShowSuccessAsync(statusMessage);
            clienteSearchTerm = selectedCliente.Identificacion;
            await SearchClientesAsync();

            var refreshed = clienteResults.FirstOrDefault(current => current.PersonaId == selectedCliente.PersonaId);
            if (refreshed is not null)
            {
                SelectCliente(refreshed);
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo activar la extension de cliente.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isCreatingClienteExtension = false;
        }
    }

    private decimal GetCartSubtotal() => cartItems.Sum(item => item.Subtotal);

    private decimal GetCartIva() => cartItems.Sum(item => item.IvaValor);

    private decimal GetCartSubtotalByRate(decimal rate)
        => cartItems.Where(item => item.PorcentajeIva == rate).Sum(item => item.Subtotal);

    private decimal GetCartIvaByRate(decimal rate)
        => cartItems.Where(item => item.PorcentajeIva == rate).Sum(item => item.IvaValor);

    private decimal GetCartTotal() => cartItems.Sum(item => item.Total);

    private void OpenLastFacturaWhatsApp()
    {
        if (lastFactura is null)
        {
            return;
        }

        showWhatsAppModal = true;
    }

    private void CloseWhatsAppModal()
    {
        showWhatsAppModal = false;
    }

    private void OpenPagoQrModal()
    {
        showPagoQrModal = true;
    }

    private void ClosePagoQrModal()
    {
        showPagoQrModal = false;
    }

    private async Task OnPagoDigitalAprobadoAsync(string transactionId)
    {
        formaPago = "19";
        showPagoQrModal = false;
        await PopupNotificationService.ShowSuccessAsync($"Pago digital confirmado: {transactionId}.");
    }

    public void Dispose()
    {
        dotNetReference?.Dispose();
    }

    private void OpenOperationalContextModal()
    {
        if (!hasCajaActiva || hasCajaVencida)
        {
            showCajaRequiredModal = true;
            showOperationalContextModal = false;
            return;
        }

        showOperationalContextModal = true;
        errorMessage = null;
        statusMessage = null;
    }

    private async Task CloseOperationalContextModal()
    {
        if (!HasOperationalContext)
        {
            return;
        }

        errorMessage = null;
        statusMessage = null;
        showOperationalContextModal = false;
        if (CanOperateSearch)
        {
            await FocusProductSearchAsync();
        }
    }

    private void SelectPuntoEmision(PosPuntoEmisionResponse punto)
    {
        selectedPuntoEmision = punto;
    }

    private bool EnsureOperationalContext()
    {
        if (HasOperationalContext && hasCajaActiva && !hasCajaVencida)
        {
            return true;
        }

        errorMessage = puntosEmision.Count == 0
            ? "La empresa activa no tiene puntos de emision configurados."
            : hasCajaVencida
                ? "Tienes una caja abierta de un dia anterior. Debes cerrarla antes de operar el POS."
                : !hasCajaActiva
                ? "Debes abrir una caja para este usuario antes de operar el POS."
                : "Debes seleccionar un establecimiento y punto de emision antes de operar el POS.";
        showCajaRequiredModal = !hasCajaActiva || hasCajaVencida;
        showOperationalContextModal = !showCajaRequiredModal && puntosEmision.Count > 0;
        return false;
    }

    private bool EnsureOperationalContextForSearch()
    {
        if (HasOperationalContext && ((hasCajaActiva && !hasCajaVencida) || !isOnline))
        {
            return true;
        }

        return EnsureOperationalContext();
    }

    private void GoToControlCaja()
    {
        NavigationManager.NavigateTo("/caja/control");
    }

    private void CloseCajaRequiredModal()
    {
        if (!hasCajaActiva || hasCajaVencida)
        {
            GoToControlCaja();
            return;
        }

        showCajaRequiredModal = false;
    }

    private void ShowPosInfoModal(string title, string message)
    {
        posInfoModalTitle = title;
        posInfoModalMessage = message;
        showPosInfoModal = true;
        errorMessage = null;
    }

    private void ClosePosInfoModal()
    {
        showPosInfoModal = false;
        posInfoModalTitle = null;
        posInfoModalMessage = null;
    }

    private async Task InitializeOfflineModeAsync()
    {
        try
        {
            isOnline = await JsRuntime.InvokeAsync<bool>("easyMarketPosOffline.isOnline");
            dotNetReference = DotNetObjectReference.Create(this);
            networkHandlerReference = await JsRuntime.InvokeAsync<IJSObjectReference>("easyMarketPosOffline.registerNetworkHandlers", dotNetReference);
            if (isOnline && selectedPuntoEmision is not null)
            {
                await RefreshOfflineCatalogAsync();
                await SyncOfflineQueueAsync();
            }
        }
        catch (JSException)
        {
            isOnline = true;
        }
    }

    [JSInvokable]
    public async Task OnBrowserOnline()
    {
        isOnline = true;
        await PopupNotificationService.ShowSuccessAsync("Conexion recuperada. Sincronizando ventas offline...");
        await RefreshOfflineCatalogAsync();
        await SyncOfflineQueueAsync();
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task OnBrowserOffline()
    {
        isOnline = false;
        await PopupNotificationService.ShowInfoAsync("Modo contingencia offline activo. Las ventas se guardaran localmente.");
        await InvokeAsync(StateHasChanged);
    }

    private async Task RefreshOfflineCatalogAsync()
    {
        if (selectedPuntoEmision is null)
        {
            return;
        }

        var catalog = await PosOfflineApiClient.GetCatalogoCacheAsync(selectedPuntoEmision.BodegaId);
        await JsRuntime.InvokeVoidAsync("easyMarketPosOffline.saveCatalogo", catalog);
    }

    private async Task SaveOfflineSaleAsync()
    {
        if (selectedCliente?.ClienteId.HasValue != true || selectedPuntoEmision is null)
        {
            errorMessage = "No se pudo guardar en contingencia: cliente o punto de emision incompleto.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
            return;
        }

        var clienteId = selectedCliente.ClienteId.GetValueOrDefault();

        var offlineSale = new VentaOfflineQueueDto
        {
            EmpresaId = Guid.Empty,
            ClienteId = clienteId,
            BodegaId = selectedPuntoEmision.BodegaId,
            Establecimiento = selectedPuntoEmision.Establecimiento,
            PuntoEmision = selectedPuntoEmision.PuntoEmision,
            FormaPago = formaPago,
            MontoRecibido = IsCashPayment ? efectivoRecibido : null,
            VueltoEntregado = IsCashPayment ? VueltoCalculado : null,
            Observacion = observacion,
            FechaHoraLocal = DateTimeOffset.Now,
            FirmaPreliminarLocal = $"CONT-{DateTimeOffset.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..30],
            Items = cartItems.Select(item => new VentaOfflineDetalleDto
            {
                ProductoId = item.ProductoId,
                Codigo = item.Codigo,
                Nombre = item.Nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioVenta,
                TarifaIVA = item.PorcentajeIva,
                UsuarioIdOperador = item.UsuarioIdOperador
            }).ToArray()
        };

        await JsRuntime.InvokeVoidAsync("easyMarketPosOffline.enqueueVenta", offlineSale);
        statusMessage = "Venta guardada localmente. Se sincronizara al recuperar internet.";
        await PopupNotificationService.ShowSuccessAsync(statusMessage);
        cartItems.Clear();
        productoResults.Clear();
        observacion = null;
        efectivoRecibido = null;
    }

    private async Task SyncOfflineQueueAsync()
    {
        if (isSyncingOfflineQueue)
        {
            return;
        }

        isSyncingOfflineQueue = true;
        try
        {
            var ventas = await JsRuntime.InvokeAsync<List<VentaOfflineQueueDto>>("easyMarketPosOffline.getVentasPendientes");
            if (ventas.Count == 0)
            {
                return;
            }

            var result = await PosOfflineApiClient.SincronizarAsync(ventas);
            foreach (var item in result.Resultados.Where(current => current.Succeeded))
            {
                await JsRuntime.InvokeVoidAsync("easyMarketPosOffline.removeVenta", item.LocalQueueId);
            }

            await PopupNotificationService.ShowInfoAsync($"Sincronizacion offline: {result.Procesadas} procesadas, {result.ConflictosStock} conflictos.");
        }
        catch (Exception exception) when (exception is HttpRequestException or JSException)
        {
            await PopupNotificationService.ShowInfoAsync("La cola offline queda pendiente para el proximo intento.");
        }
        finally
        {
            isSyncingOfflineQueue = false;
        }
    }

    private sealed class CartItemModel
    {
        public Guid RowId { get; set; }

        public Guid ProductoId { get; set; }

        public Guid? UsuarioIdOperador { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public decimal Cantidad { get; set; } = 1;

        public decimal PrecioVenta { get; set; }

        public decimal PorcentajeIva { get; set; }

        public bool ControlaStock { get; set; }

        public decimal StockDisponible { get; set; }

        public bool IsService => !ControlaStock;

        public decimal Subtotal => Math.Round(Cantidad * PrecioVenta, 2);

        public decimal IvaValor => Math.Round(Subtotal * (PorcentajeIva / 100m), 2);

        public decimal Total => Subtotal + IvaValor;
    }
}

