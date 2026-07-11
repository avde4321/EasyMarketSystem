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
    private readonly List<BodegaResponse> bodegas = [];
    private ProductoRequest productoRequest = new();
    private AjusteStockRequest ajusteRequest = new();
    private EgresoMermaRequest mermaRequest = new();
    private TransferenciaInventarioRequest transferenciaRequest = new();
    private ProductoResponse? selectedProduct;
    private ProductoResponse? selectedAdjustmentProduct;
    private ProductoResponse? selectedMermaProduct;
    private ProductoResponse? selectedTransferProduct;
    private ProductoResponse? editingProductSnapshot;
    private Guid? editingProductId;
    private bool isLoading = true;
    private bool isKardexLoading;
    private bool isSaving;
    private bool isProductModalOpen;
    private bool isAdjustmentModalOpen;
    private bool isMermaModalOpen;
    private bool isTransferModalOpen;
    private string? errorMessage;
    private string searchTerm = string.Empty;
    private string kardexSearchTerm = string.Empty;
    private string selectedBodegaIdString = string.Empty;
    private string selectedKardexBodegaIdString = string.Empty;
    private string transferenciaDestinoIdString = string.Empty;
    private const int PageSize = 10;
    private int totalCount;
    private int currentSkip;

    private IEnumerable<ProductoResponse> VisibleProductos => productos;
    private IReadOnlyList<BodegaResponse> activeBodegas => bodegas.Where(bodega => bodega.IsActive).ToList();
    private BodegaResponse? selectedBodega => Guid.TryParse(selectedBodegaIdString, out var id)
        ? activeBodegas.FirstOrDefault(bodega => bodega.Id == id)
        : activeBodegas.FirstOrDefault();
    private Guid? selectedBodegaId => selectedBodega?.Id;
    private Guid? selectedKardexBodegaId => Guid.TryParse(selectedKardexBodegaIdString, out var id) ? id : null;
    private Guid? selectedTransferDestinoId => Guid.TryParse(transferenciaDestinoIdString, out var id) ? id : null;
    private IReadOnlyList<BodegaResponse> destinationBodegas => activeBodegas
        .Where(bodega => bodega.Id != selectedBodegaId)
        .ToList();
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
    private bool CanSubmitMerma => selectedMermaProduct is not null &&
                                   mermaRequest.Cantidad > 0 &&
                                   mermaRequest.Cantidad <= selectedMermaProduct.StockActual;
    private bool CanSubmitTransfer => selectedTransferProduct is not null &&
                                      selectedTransferDestinoId.HasValue &&
                                      selectedTransferDestinoId.Value != Guid.Empty &&
                                      transferenciaRequest.Cantidad > 0 &&
                                      transferenciaRequest.Cantidad <= selectedTransferProduct.StockActual;

    private IEnumerable<KardexMovimientoResponse> FilteredKardex => kardex.Where(movimiento =>
        string.IsNullOrWhiteSpace(kardexSearchTerm) ||
        movimiento.TipoMovimiento.Contains(kardexSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        movimiento.Concepto.Contains(kardexSearchTerm, StringComparison.OrdinalIgnoreCase) ||
        (movimiento.Referencia?.Contains(kardexSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (movimiento.BodegaNombre?.Contains(kardexSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        await LoadBodegasAsync();
        await LoadProductsAsync(resetPaging: true);
    }

    private async Task LoadBodegasAsync()
    {
        bodegas.Clear();
        var response = await InventarioApiClient.GetBodegasAsync();
        bodegas.AddRange(response.Where(bodega => bodega.IsActive));

        if (!bodegas.Any())
        {
            errorMessage = "No existen bodegas activas configuradas para la empresa.";
            selectedBodegaIdString = string.Empty;
            return;
        }

        if (!Guid.TryParse(selectedBodegaIdString, out var currentId) || bodegas.All(bodega => bodega.Id != currentId))
        {
            var preferred = bodegas
                .OrderByDescending(bodega => string.Equals(bodega.Nombre, "Principal", StringComparison.OrdinalIgnoreCase))
                .ThenBy(bodega => bodega.Nombre)
                .First();

            selectedBodegaIdString = preferred.Id.ToString();
        }
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
            var page = await InventarioApiClient.GetProductosAsync(searchTerm, currentSkip, PageSize, selectedBodegaId);
            productos.Clear();
            productos.AddRange(page.Items);
            totalCount = page.TotalCount;

            if (selectedProduct is not null)
            {
                selectedProduct = productos.FirstOrDefault(producto => producto.Id == selectedProduct.Id) ?? selectedProduct;
            }
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

    private async Task LoadKardexAsync(ProductoResponse producto, bool preserveCurrentFilter = false)
    {
        selectedProduct = producto;
        isKardexLoading = true;

        if (!preserveCurrentFilter)
        {
            selectedKardexBodegaIdString = string.Empty;
        }

        try
        {
            kardex.Clear();
            kardex.AddRange(await InventarioApiClient.GetKardexAsync(producto.Id, selectedKardexBodegaId));
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el Kardex del producto seleccionado.";
        }
        finally
        {
            isKardexLoading = false;
        }
    }

    private async Task OnBodegaChangedAsync()
    {
        await LoadProductsAsync(resetPaging: true);

        if (selectedProduct is not null)
        {
            var refreshed = productos.FirstOrDefault(producto => producto.Id == selectedProduct.Id);
            if (refreshed is not null)
            {
                await LoadKardexAsync(refreshed, preserveCurrentFilter: true);
            }
        }
    }

    private async Task OnKardexBodegaChangedAsync()
    {
        if (selectedProduct is not null)
        {
            await LoadKardexAsync(selectedProduct, preserveCurrentFilter: true);
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

    private void OpenAdjustmentModal(ProductoResponse producto)
    {
        if (selectedBodegaId is null)
        {
            errorMessage = "Debe seleccionar una bodega operativa antes de ajustar stock.";
            return;
        }

        selectedAdjustmentProduct = producto;
        ajusteRequest = new AjusteStockRequest
        {
            BodegaId = selectedBodegaId,
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
        ajusteRequest.BodegaId = selectedBodegaId;

        try
        {
            var result = await InventarioApiClient.AjustarStockAsync(selectedAdjustmentProduct.Id, ajusteRequest);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseAdjustmentModal();
            await RefreshAfterInventoryMutationAsync(selectedAdjustmentProduct.Id);
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

    private void OpenMermaModal(ProductoResponse producto)
    {
        if (selectedBodegaId is null)
        {
            errorMessage = "Debe seleccionar una bodega operativa antes de registrar una merma.";
            return;
        }

        selectedMermaProduct = producto;
        mermaRequest = new EgresoMermaRequest
        {
            ProductoId = producto.Id,
            BodegaId = selectedBodegaId.Value,
            Motivo = "Merma"
        };
        isMermaModalOpen = true;
        errorMessage = null;
    }

    private void CloseMermaModal()
    {
        isMermaModalOpen = false;
        selectedMermaProduct = null;
        isSaving = false;
    }

    private async Task SaveMermaAsync()
    {
        if (selectedMermaProduct is null || selectedBodegaId is null)
        {
            return;
        }

        if (!CanSubmitMerma)
        {
            errorMessage = "La merma no puede dejar el stock de la bodega en negativo.";
            return;
        }

        isSaving = true;
        errorMessage = null;
        mermaRequest.ProductoId = selectedMermaProduct.Id;
        mermaRequest.BodegaId = selectedBodegaId.Value;

        try
        {
            var result = await InventarioApiClient.RegistrarMermaAsync(mermaRequest);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseMermaModal();
            await RefreshAfterInventoryMutationAsync(selectedMermaProduct.Id);
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo registrar la merma.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private void OpenTransferModal(ProductoResponse producto)
    {
        if (selectedBodegaId is null)
        {
            errorMessage = "Debe seleccionar una bodega operativa antes de transferir stock.";
            return;
        }

        selectedTransferProduct = producto;
        transferenciaRequest = new TransferenciaInventarioRequest
        {
            ProductoId = producto.Id,
            BodegaOrigenId = selectedBodegaId.Value
        };
        transferenciaDestinoIdString = string.Empty;
        isTransferModalOpen = true;
        errorMessage = null;
    }

    private void CloseTransferModal()
    {
        isTransferModalOpen = false;
        selectedTransferProduct = null;
        transferenciaDestinoIdString = string.Empty;
        isSaving = false;
    }

    private async Task SaveTransferAsync()
    {
        if (selectedTransferProduct is null || selectedBodegaId is null || !selectedTransferDestinoId.HasValue)
        {
            return;
        }

        if (!CanSubmitTransfer)
        {
            errorMessage = "La transferencia requiere una bodega destino valida y no puede dejar el stock en negativo.";
            return;
        }

        isSaving = true;
        errorMessage = null;
        transferenciaRequest.ProductoId = selectedTransferProduct.Id;
        transferenciaRequest.BodegaOrigenId = selectedBodegaId.Value;
        transferenciaRequest.BodegaDestinoId = selectedTransferDestinoId.Value;

        try
        {
            var result = await InventarioApiClient.TransferirStockAsync(transferenciaRequest);
            if (!result.Succeeded)
            {
                errorMessage = result.ErrorMessage;
                return;
            }

            CloseTransferModal();
            await RefreshAfterInventoryMutationAsync(selectedTransferProduct.Id);
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo registrar la transferencia.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task RefreshAfterInventoryMutationAsync(Guid productoId)
    {
        await LoadProductsAsync();
        var refreshed = productos.FirstOrDefault(producto => producto.Id == productoId);
        if (refreshed is not null)
        {
            await LoadKardexAsync(refreshed, preserveCurrentFilter: true);
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

    private static string BuildBodegaLabel(BodegaResponse bodega)
    {
        return string.IsNullOrWhiteSpace(bodega.Direccion)
            ? bodega.Nombre
            : $"{bodega.Nombre} - {bodega.Direccion}";
    }
}
