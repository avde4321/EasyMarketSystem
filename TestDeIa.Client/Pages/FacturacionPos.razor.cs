using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Client.Pages;

public partial class FacturacionPos : IDisposable
{
    [Inject]
    private FacturacionApiClient FacturacionApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    private readonly List<PosClienteResponse> clienteResults = [];
    private readonly List<PosProductoResponse> productoResults = [];
    private readonly List<CatalogoItemResponse> formasPago = [];
    private readonly List<CartItemModel> cartItems = [];
    private string clienteSearchTerm = string.Empty;
    private string productoSearchTerm = string.Empty;
    private PosClienteResponse? selectedCliente;
    private string formaPago = "Efectivo";
    private string? observacion;
    private bool isSubmitting;
    private string? errorMessage;
    private string? statusMessage;
    private const int SearchPageSize = 8;
    private int clienteSkip;
    private int productoSkip;
    private int clienteTotalCount;
    private int productoTotalCount;
    private bool CanGoPreviousClientes => clienteSkip > 0;
    private bool CanGoNextClientes => clienteSkip + SearchPageSize < clienteTotalCount;
    private bool CanGoPreviousProductos => productoSkip > 0;
    private bool CanGoNextProductos => productoSkip + SearchPageSize < productoTotalCount;

    protected override async Task OnInitializedAsync()
    {
        await LoadCatalogosAsync();
    }

    private async Task LoadCatalogosAsync()
    {
        formasPago.Clear();
        formasPago.AddRange(await CatalogosApiClient.GetItemsAsync("FORMA_PAGO_SRI", true));
        formaPago = formasPago.FirstOrDefault()?.Codigo ?? "Efectivo";
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

        if (string.IsNullOrWhiteSpace(productoSearchTerm) || productoSearchTerm.Trim().Length < 2)
        {
            return;
        }

        var page = await FacturacionApiClient.SearchProductosAsync(productoSearchTerm.Trim(), productoSkip, SearchPageSize);
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
            Cantidad = 1
        });
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

        isSubmitting = true;

        try
        {
            var result = await FacturacionApiClient.EmitirFacturaAsync(new EmitirFacturaRequest
            {
                ClienteId = selectedCliente.ClienteId,
                FormaPago = formaPago,
                Observacion = observacion,
                Items = cartItems.Select(item => new EmitirFacturaDetalleRequest
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad
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

    private decimal GetCartSubtotal() => cartItems.Sum(item => item.Subtotal);

    private decimal GetCartIva() => cartItems.Sum(item => item.IvaValor);

    private decimal GetCartTotal() => cartItems.Sum(item => item.Total);

    public void Dispose()
    {
    }

    private sealed class CartItemModel
    {
        public Guid ProductoId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public decimal Cantidad { get; set; } = 1;

        public decimal PrecioVenta { get; set; }

        public decimal PorcentajeIva { get; set; }

        public decimal Subtotal => Math.Round(Cantidad * PrecioVenta, 2);

        public decimal IvaValor => Math.Round(Subtotal * (PorcentajeIva / 100m), 2);

        public decimal Total => Subtotal + IvaValor;
    }
}
