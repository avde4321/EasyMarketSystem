using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Shared.Requests.Inventario;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Pages;

public partial class ProductoMantenimiento
{
    [Inject]
    private InventarioApiClient InventarioApiClient { get; set; } = default!;

    private readonly List<ProductoResponse> productos = [];
    private readonly string[] unidadesMedida = ["Unidad", "Caja", "Paquete", "Kg", "Litro", "Hora", "Servicio"];
    private ProductoRequest productoRequest = new();
    private Guid? editingProductId;
    private bool isLoading = true;
    private bool isSaving;
    private bool isModalOpen;
    private string searchTerm = string.Empty;
    private string? dialogMessage;
    private string categoriaIdText = string.Empty;
    private string tarifaIva = "15";
    private const int PageSize = 12;
    private int currentSkip;
    private int totalCount;

    private bool CanGoPrevious => currentSkip > 0;
    private bool CanGoNext => currentSkip + PageSize < totalCount;
    private int PageNumber => (currentSkip / PageSize) + 1;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
    private bool IsMercaderia => string.Equals(productoRequest.NaturalezaItem, "Mercaderia", StringComparison.OrdinalIgnoreCase);
    private bool IsServicio => string.Equals(productoRequest.NaturalezaItem, "Servicio", StringComparison.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync(resetPaging: true);
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
            var page = await InventarioApiClient.GetProductosAsync(searchTerm, currentSkip, PageSize);
            productos.Clear();
            productos.AddRange(page.Items);
            totalCount = page.TotalCount;
        }
        finally
        {
            isLoading = false;
        }
    }

    private Task SearchAsync() => LoadAsync(resetPaging: true);

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

    private void OpenCreateModal()
    {
        editingProductId = null;
        productoRequest = new ProductoRequest
        {
            UnidadMedida = "Unidad",
            NaturalezaItem = "Mercaderia",
            ControlaStock = true,
            CodigoIva = "IVA_15",
            PorcentajeIva = 15,
            IsActive = true,
            StockMinimo = 0
        };
        categoriaIdText = string.Empty;
        tarifaIva = "15";
        dialogMessage = null;
        isModalOpen = true;
    }

    private void OpenEditModal(ProductoResponse producto)
    {
        editingProductId = producto.Id;
        productoRequest = new ProductoRequest
        {
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            CategoriaId = producto.CategoriaId,
            UnidadMedida = string.IsNullOrWhiteSpace(producto.UnidadMedida) ? "Unidad" : producto.UnidadMedida,
            NaturalezaItem = string.IsNullOrWhiteSpace(producto.NaturalezaItem) ? (producto.ControlaStock ? "Mercaderia" : "Servicio") : producto.NaturalezaItem,
            CodigoIva = producto.CodigoIva,
            PorcentajeIva = producto.PorcentajeIva,
            PrecioVenta = producto.PrecioVenta,
            CostoReferencial = producto.CostoReferencial,
            StockMinimo = producto.StockMinimo,
            ControlaStock = producto.ControlaStock,
            AplicaComision = producto.AplicaComision,
            TipoComision = producto.TipoComision ?? "Porcentaje",
            ValorComision = producto.ValorComision,
            IsActive = producto.IsActive
        };
        categoriaIdText = producto.CategoriaId?.ToString() ?? string.Empty;
        tarifaIva = producto.PorcentajeIva == 0 ? "0" : "15";
        OnNaturalezaChanged();
        dialogMessage = null;
        isModalOpen = true;
    }

    private void CloseModal()
    {
        isModalOpen = false;
        isSaving = false;
        dialogMessage = null;
    }

    private async Task SaveAsync()
    {
        dialogMessage = null;
        isSaving = true;

        try
        {
            if (!string.IsNullOrWhiteSpace(categoriaIdText) && Guid.TryParse(categoriaIdText, out var categoriaId))
            {
                productoRequest.CategoriaId = categoriaId;
            }
            else
            {
                productoRequest.CategoriaId = null;
            }

            OnNaturalezaChanged();
            OnTarifaIvaChanged();

            var result = editingProductId.HasValue
                ? await InventarioApiClient.UpdateProductoAsync(editingProductId.Value, productoRequest)
                : await InventarioApiClient.CreateProductoAsync(productoRequest);

            if (!result.Succeeded)
            {
                dialogMessage = result.ErrorMessage ?? "No se pudo guardar el item.";
                return;
            }

            CloseModal();
            await LoadAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            dialogMessage = ex.Message;
        }
        finally
        {
            isSaving = false;
        }
    }

    private void OnNaturalezaChanged()
    {
        productoRequest.ControlaStock = IsMercaderia;
        if (!IsMercaderia)
        {
            productoRequest.StockMinimo = null;
            productoRequest.StockInicial = 0;
            productoRequest.CostoInicial = 0;
        }

        if (!IsServicio)
        {
            productoRequest.AplicaComision = false;
            productoRequest.TipoComision = null;
            productoRequest.ValorComision = null;
        }
        else
        {
            productoRequest.UnidadMedida = string.Equals(productoRequest.UnidadMedida, "Unidad", StringComparison.OrdinalIgnoreCase)
                ? "Servicio"
                : productoRequest.UnidadMedida;
            productoRequest.TipoComision ??= "Porcentaje";
        }
    }

    private void OnTarifaIvaChanged()
    {
        productoRequest.PorcentajeIva = tarifaIva == "0" ? 0 : 15;
        productoRequest.CodigoIva = tarifaIva == "0" ? "IVA_0" : "IVA_15";
    }

    private static string FormatNature(string? naturalezaItem) => naturalezaItem switch
    {
        "Servicio" => "Servicio",
        "ActivoFijo" => "Activo fijo",
        _ => "Mercaderia"
    };

    private static string GetNatureBadgeClass(string? naturalezaItem) => naturalezaItem switch
    {
        "Servicio" => "status-servicio",
        "ActivoFijo" => "status-activo-fijo",
        _ => string.Empty
    };
}
