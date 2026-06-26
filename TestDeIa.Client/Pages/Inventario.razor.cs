using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Pages;

public partial class Inventario
{
    [Inject]
    private InventarioApiClient InventarioApiClient { get; set; } = default!;

    private readonly List<ProductoResponse> productos = [];
    private readonly List<KardexMovimientoResponse> kardex = [];
    private ProductoRequest productoRequest = new();
    private AjusteStockRequest ajusteRequest = new();
    private ProductoResponse? selectedProduct;
    private ProductoResponse? selectedAdjustmentProduct;
    private ProductoResponse? editingProductSnapshot;
    private Guid? editingProductId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isProductModalOpen;
    private bool isAdjustmentModalOpen;
    private string? errorMessage;
    private string searchTerm = string.Empty;
    private string kardexSearchTerm = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;
    private IEnumerable<ProductoResponse> VisibleProductos => productos;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

    private IEnumerable<KardexMovimientoResponse> FilteredKardex => kardex.Where(movimiento =>
        string.IsNullOrWhiteSpace(kardexSearchTerm) ||
        movimiento.TipoMovimiento.Contains(kardexSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        movimiento.Concepto.Contains(kardexSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        (movimiento.Referencia?.Contains(kardexSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync(resetPaging: true);
    }

    private async Task LoadProductsAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var page = await InventarioApiClient.GetProductosAsync(searchTerm, currentSkip, PageSize);
            productos.Clear();
            productos.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el catalogo de productos.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenProductModal()
    {
        editingProductId = null;
        editingProductSnapshot = null;
        productoRequest = new ProductoRequest();
        isProductModalOpen = true;
        errorMessage = null;
    }

    private void OpenEditProductModal(ProductoResponse producto)
    {
        editingProductId = producto.Id;
        editingProductSnapshot = producto;
        productoRequest = new ProductoRequest
        {
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            CodigoIva = producto.CodigoIva,
            PorcentajeIva = producto.PorcentajeIva,
            PrecioVenta = producto.PrecioVenta,
            StockMinimo = producto.StockMinimo,
            ControlaStock = producto.ControlaStock,
            IsActive = producto.IsActive
        };
        isProductModalOpen = true;
        errorMessage = null;
    }

    private void CloseProductModal()
    {
        isProductModalOpen = false;
        editingProductSnapshot = null;
        isSaving = false;
    }

    private async Task SaveProductAsync()
    {
        isSaving = true;
        errorMessage = null;

        try
        {
            var result = editingProductId.HasValue
                ? await InventarioApiClient.UpdateProductoAsync(editingProductId.Value, productoRequest)
                : await InventarioApiClient.CreateProductoAsync(productoRequest);

            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseProductModal();
            await LoadProductsAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo guardar el producto.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task LoadKardexAsync(ProductoResponse producto)
    {
        selectedProduct = producto;
        kardex.Clear();
        kardex.AddRange(await InventarioApiClient.GetKardexAsync(producto.Id));
    }

    private void OpenAdjustmentModal(ProductoResponse producto)
    {
        selectedAdjustmentProduct = producto;
        ajusteRequest = new AjusteStockRequest
        {
            TipoMovimiento = "Entrada",
            Concepto = "Ajuste manual"
        };
        isAdjustmentModalOpen = true;
        errorMessage = null;
    }

    private void CloseAdjustmentModal()
    {
        isAdjustmentModalOpen = false;
        selectedAdjustmentProduct = null;
        isSaving = false;
    }

    private async Task SaveAdjustmentAsync()
    {
        if (selectedAdjustmentProduct is null)
        {
            return;
        }

        isSaving = true;
        errorMessage = null;

        try
        {
            var result = await InventarioApiClient.AjustarStockAsync(selectedAdjustmentProduct.Id, ajusteRequest);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseAdjustmentModal();
            await LoadProductsAsync();
            var refreshed = productos.FirstOrDefault(producto => producto.Id == selectedProduct?.Id);
            if (refreshed is not null)
            {
                await LoadKardexAsync(refreshed);
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo registrar el ajuste.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task SearchProductsAsync()
    {
        await LoadProductsAsync(resetPaging: true);
    }

    private void OnControlaStockChanged(bool value)
    {
        productoRequest.ControlaStock = value;

        if (!value)
        {
            productoRequest.StockMinimo = 0;
            productoRequest.StockInicial = 0;
            productoRequest.CostoInicial = 0;
        }
    }

    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadProductsAsync();
    }

    private async Task GoToNextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadProductsAsync();
    }
}
