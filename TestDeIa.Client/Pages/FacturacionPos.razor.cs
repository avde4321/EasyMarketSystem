using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TestDeIa.Client.Services.Caja;
using TestDeIa.Client.Services.Clientes;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Shared.Requests.Clientes;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Security;

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

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private readonly List<PosClienteResponse> clienteResults = [];
    private readonly List<PosProductoResponse> productoResults = [];
    private readonly List<PosPuntoEmisionResponse> puntosEmision = [];
    private readonly List<CatalogoItemResponse> formasPago = [];
    private readonly List<CartItemModel> cartItems = [];
    private string clienteSearchTerm = string.Empty;
    private string productoSearchTerm = string.Empty;
    private PosClienteResponse? selectedCliente;
    private PosPuntoEmisionResponse? selectedPuntoEmision;
    private string formaPago = "01";
    private string? observacion;
    private bool isSubmitting;
    private bool showOperationalContextModal;
    private bool isCreatingClienteExtension;
    private string? errorMessage;
    private string? statusMessage;
    private bool isCajaLoading;
    private bool hasCajaActiva;
    private bool canEditServicePrice;
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
    private bool CanOperatePos => HasOperationalContext && hasCajaActiva;

    protected override async Task OnInitializedAsync()
    {
        await ResolveCurrentUserCapabilitiesAsync();
        await LoadCatalogosAsync();
        await LoadPuntosEmisionAsync();
        await LoadCajaStateAsync();
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
        canEditServicePrice = user.Identity?.IsAuthenticated == true && user.IsInRole(SecurityRoleNames.Administrador);
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
        showOperationalContextModal = true;
    }

    private async Task LoadCajaStateAsync()
    {
        isCajaLoading = true;

        try
        {
            hasCajaActiva = await CajaApiClient.GetActivaAsync() is not null;
        }
        catch (HttpRequestException)
        {
            hasCajaActiva = false;
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

        if (!EnsureOperationalContext())
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
        productoSkip = 0;
        await LoadProductosPageAsync();
    }

    private async Task LoadProductosPageAsync()
    {
        errorMessage = null;
        statusMessage = null;
        productoResults.Clear();

        if (!EnsureOperationalContext())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(productoSearchTerm) || productoSearchTerm.Trim().Length < 2)
        {
            return;
        }

        var page = await FacturacionApiClient.SearchProductosAsync(productoSearchTerm.Trim(), productoSkip, SearchPageSize, selectedPuntoEmision?.BodegaId);
        productoResults.AddRange(page.Items);
        productoTotalCount = page.TotalCount;
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

        var existing = cartItems.FirstOrDefault(item => item.ProductoId == producto.ProductoId);
        if (existing is not null)
        {
            existing.Cantidad += 1;
            return;
        }

        cartItems.Add(new CartItemModel
        {
            ProductoId = producto.ProductoId,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            PrecioVenta = producto.PrecioVenta,
            PorcentajeIva = producto.PorcentajeIva,
            ControlaStock = producto.ControlaStock,
            Cantidad = 1
        });
    }

    private void IncrementQuantity(CartItemModel item)
    {
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

    private void RemoveProducto(Guid productoId)
    {
        var existing = cartItems.FirstOrDefault(item => item.ProductoId == productoId);
        if (existing is not null)
        {
            cartItems.Remove(existing);
        }
    }

    private async Task CobrarAsync()
    {
        errorMessage = null;
        statusMessage = null;

        if (selectedCliente is null)
        {
            errorMessage = "Selecciona un cliente antes de cobrar.";
            return;
        }

        if (!selectedCliente.HasClienteExtension || !selectedCliente.ClienteId.HasValue)
        {
            errorMessage = "La persona seleccionada aun no tiene el rol de cliente en esta empresa. Activalo antes de facturar.";
            return;
        }

        if (!EnsureOperationalContext())
        {
            return;
        }

        if (cartItems.Count == 0)
        {
            errorMessage = "Agrega al menos un producto al carrito.";
            return;
        }

        if (cartItems.Any(item => item.Cantidad <= 0))
        {
            errorMessage = "Todas las cantidades deben ser mayores a cero.";
            return;
        }

        if (cartItems.Any(item => item.IsService && item.PrecioVenta < 0))
        {
            errorMessage = "El precio de los servicios no puede ser negativo.";
            return;
        }

        isSubmitting = true;

        try
        {
            var result = await FacturacionApiClient.EmitirFacturaAsync(new EmitirFacturaRequest
            {
                ClienteId = selectedCliente.ClienteId.Value,
                BodegaId = selectedPuntoEmision!.BodegaId,
                Establecimiento = selectedPuntoEmision!.Establecimiento,
                PuntoEmision = selectedPuntoEmision.PuntoEmision,
                FormaPago = formaPago,
                Observacion = observacion,
                Items = cartItems.Select(item => new EmitirFacturaDetalleRequest
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitarioOverride = item.IsService ? item.PrecioVenta : null
                }).ToArray()
            });

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            statusMessage = $"{result.Data?.NumeroComprobante} registrada en estado pendiente. La validacion SRI sigue en segundo plano.";
            cartItems.Clear();
            productoResults.Clear();
            observacion = null;
            await LoadCajaStateAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo registrar la factura.";
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

    public void Dispose()
    {
    }

    private void OpenOperationalContextModal()
    {
        showOperationalContextModal = true;
        errorMessage = null;
        statusMessage = null;
    }

    private void CloseOperationalContextModal()
    {
        if (!HasOperationalContext)
        {
            return;
        }

        errorMessage = null;
        statusMessage = null;
        showOperationalContextModal = false;
    }

    private void SelectPuntoEmision(PosPuntoEmisionResponse punto)
    {
        selectedPuntoEmision = punto;
    }

    private bool EnsureOperationalContext()
    {
        if (HasOperationalContext && hasCajaActiva)
        {
            return true;
        }

        errorMessage = puntosEmision.Count == 0
            ? "La empresa activa no tiene puntos de emision configurados."
            : !hasCajaActiva
                ? "Debes abrir una caja para este usuario antes de operar el POS."
                : "Debes seleccionar un establecimiento y punto de emision antes de operar el POS.";
        showOperationalContextModal = puntosEmision.Count > 0;
        return false;
    }

    private sealed class CartItemModel
    {
        public Guid ProductoId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public decimal Cantidad { get; set; } = 1;

        public decimal PrecioVenta { get; set; }

        public decimal PorcentajeIva { get; set; }

        public bool ControlaStock { get; set; }

        public bool IsService => !ControlaStock;

        public decimal Subtotal => Math.Round(Cantidad * PrecioVenta, 2);

        public decimal IvaValor => Math.Round(Subtotal * (PorcentajeIva / 100m), 2);

        public decimal Total => Subtotal + IvaValor;
    }
}