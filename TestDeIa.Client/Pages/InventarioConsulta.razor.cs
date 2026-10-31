using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Pages;

public partial class InventarioConsulta
{
    [Inject]
    private InventarioApiClient InventarioApiClient { get; set; } = default!;

    private readonly List<ProductoResponse> productos = [];
    private readonly List<ProductoResponse> kpiProductos = [];
    private readonly List<BodegaResponse> bodegas = [];
    private readonly List<KardexMovimientoResponse> kardex = [];
    private ProductoResponse? selectedProduct;
    private bool isLoading = true;
    private bool isKardexLoading;
    private bool isKardexModalOpen;
    private string searchTerm = string.Empty;
    private string selectedBodegaIdString = string.Empty;
    private const int PageSize = 12;
    private int currentSkip;
    private int totalCount;

    private IReadOnlyList<BodegaResponse> activeBodegas => bodegas.Where(bodega => bodega.IsActive).ToList();
    private Guid? selectedBodegaId => Guid.TryParse(selectedBodegaIdString, out var id) ? id : null;
    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
    private decimal ValorTotalInventario => kpiProductos.Where(IsInventariable).Sum(producto => producto.StockActual * producto.CostoPromedio);
    private int ItemsCriticos => kpiProductos.Count(producto => IsInventariable(producto) && producto.StockActual > 0 && producto.StockMinimo.HasValue && producto.StockActual <= producto.StockMinimo.Value);
    private int ProductosAgotados => kpiProductos.Count(producto => IsInventariable(producto) && producto.StockActual == 0);
    private string StockScopeLabel => selectedBodegaId.HasValue
        ? $"Stock visible: {activeBodegas.FirstOrDefault(bodega => bodega.Id == selectedBodegaId.Value)?.Nombre ?? "Bodega seleccionada"}"
        : "Stock visible: todas las bodegas";

    protected override async Task OnInitializedAsync()
    {
        await LoadBodegasAsync();
        await LoadAsync(resetPaging: true);
    }

    private async Task LoadBodegasAsync()
    {
        bodegas.Clear();
        bodegas.AddRange(await InventarioApiClient.GetBodegasAsync());
    }

    private async Task LoadAsync(bool resetPaging = false)
    {
        if (resetPaging)
        {
            currentSkip = 0;
        }

        isLoading = true;
        try
        {
            var page = await InventarioApiClient.GetProductosAsync(searchTerm, currentSkip, PageSize, selectedBodegaId);
            productos.Clear();
            productos.AddRange(page.Items.Where(IsInventariable));
            totalCount = page.TotalCount;

            var kpiPage = await InventarioApiClient.GetProductosAsync(searchTerm, 0, 1000, selectedBodegaId);
            kpiProductos.Clear();
            kpiProductos.AddRange(kpiPage.Items.Where(IsInventariable));
        }
        finally
        {
            isLoading = false;
        }
    }

    private Task SearchAsync() => LoadAsync(resetPaging: true);

    private Task OnBodegaChangedAsync() => LoadAsync(resetPaging: true);

    private async Task PreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        currentSkip = Math.Max(0, currentSkip - PageSize);
        await LoadAsync();
    }

    private async Task NextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        currentSkip += PageSize;
        await LoadAsync();
    }

    private async Task OpenKardexModalAsync(ProductoResponse producto)
    {
        selectedProduct = producto;
        isKardexModalOpen = true;
        isKardexLoading = true;
        kardex.Clear();

        try
        {
            var movimientos = await InventarioApiClient.GetKardexAsync(producto.Id, selectedBodegaId);
            kardex.AddRange(movimientos
                .OrderByDescending(movimiento => movimiento.FechaMovimiento)
                .Take(20));
        }
        finally
        {
            isKardexLoading = false;
        }
    }

    private void CloseKardexModal()
    {
        isKardexModalOpen = false;
        selectedProduct = null;
        kardex.Clear();
    }

    private static bool IsInventariable(ProductoResponse producto) =>
        producto.ControlaStock || string.Equals(producto.NaturalezaItem, "Mercaderia", StringComparison.OrdinalIgnoreCase);

    private static string GetStockStatus(ProductoResponse producto)
    {
        if (producto.StockActual == 0)
        {
            return "Agotado";
        }

        if (producto.StockMinimo.HasValue && producto.StockActual <= producto.StockMinimo.Value)
        {
            return "Stock critico";
        }

        return "En stock";
    }

    private static string GetStockBadgeClass(ProductoResponse producto)
    {
        if (producto.StockActual == 0)
        {
            return "stock-empty";
        }

        return producto.StockMinimo.HasValue && producto.StockActual <= producto.StockMinimo.Value
            ? "stock-critical"
            : "stock-ok";
    }

    private static string FormatCategoria(Guid? categoriaId) =>
        categoriaId.HasValue ? categoriaId.Value.ToString()[..8] : "Sin categoria";

    private static string BuildBodegaLabel(BodegaResponse bodega) =>
        bodega.IsActive ? bodega.Nombre : $"{bodega.Nombre} (inactiva)";

    private static string GetKardexDocumento(KardexMovimientoResponse movimiento) =>
        string.IsNullOrWhiteSpace(movimiento.Referencia) ? "Movimiento manual" : movimiento.Referencia;
}
