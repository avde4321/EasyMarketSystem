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

    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            productos.Clear();
            productos.AddRange(await InventarioApiClient.GetProductosAsync());
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
}
