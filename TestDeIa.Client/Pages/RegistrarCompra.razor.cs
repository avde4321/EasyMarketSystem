using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Shared.Compras;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Responses.Inventario;

namespace TestDeIa.Client.Pages;

public partial class RegistrarCompra
{
    [Inject]
    private ComprasApiClient ComprasApiClient { get; set; } = default!;

    [Inject]
    private ProveedoresApiClient ProveedoresApiClient { get; set; } = default!;

    [Inject]
    private InventarioApiClient InventarioApiClient { get; set; } = default!;

    [Inject]
    private FacturacionApiClient FacturacionApiClient { get; set; } = default!;

    [Inject]
    private CatalogosApiClient CatalogosApiClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly List<ProveedorResponse> proveedores = [];
    private readonly List<BodegaResponse> bodegas = [];
    private readonly List<PosPuntoEmisionResponse> puntosEmision = [];
    private readonly List<CatalogoItemResponse> formasPago = [];
    private readonly List<ProductoResponse> productSearchResults = [];
    private readonly List<CompraDetalleRowModel> detailRows = [];
    private RegistrarCompraRequest request = new();
    private string productSearchTerm = string.Empty;
    private bool isLoadingProducts;
    private bool isSaving;
    private string? errorMessage;
    private string? successMessage;
    private DateTime fechaEmisionLocal = DateTime.Today;
    private bool isLiquidacionRoute;

    private bool IsLiquidacion => request.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra;
    private bool IsNotaVenta => request.TipoDocumentoCodigo == CompraDocumentTypes.NotaVentaRimpe;
    private bool IsFacturaProveedor => request.TipoDocumentoCodigo == CompraDocumentTypes.FacturaProveedor;
    private decimal SubtotalIva0 => CalculateSubtotalByRate(0m);
    private decimal SubtotalIva5 => IsNotaVenta ? 0 : CalculateSubtotalByRate(5m);
    private decimal SubtotalIva8 => IsNotaVenta ? 0 : CalculateSubtotalByRate(8m);
    private decimal SubtotalIva15 => IsNotaVenta ? 0 : CalculateSubtotalByRate(15m);
    private decimal TotalDescuento => Math.Round(detailRows.Sum(row => SafeValue(row.Descuento)), 2, MidpointRounding.AwayFromZero);
    private decimal TotalImpuestos => Math.Round(detailRows.Sum(row => row.TotalImpuesto), 2, MidpointRounding.AwayFromZero);
    private decimal ImporteTotal => Math.Round(detailRows.Sum(row => row.TotalLinea), 2, MidpointRounding.AwayFromZero);
    private bool CanSave => detailRows.Count > 0 && request.ProveedorId != Guid.Empty && request.BodegaId != Guid.Empty && !isLoadingProducts;

    protected override async Task OnInitializedAsync()
    {
        request = BuildEmptyRequest();
        isLiquidacionRoute = NavigationManager.Uri.Contains("/compras/liquidaciones", StringComparison.OrdinalIgnoreCase);
        if (isLiquidacionRoute)
        {
            request.TipoDocumentoCodigo = CompraDocumentTypes.LiquidacionCompra;
        }

        await LoadLookupsAsync();
        await LoadProductsAsync();
    }

    private async Task LoadLookupsAsync()
    {
        try
        {
            var proveedoresPage = await ProveedoresApiClient.GetPagedAsync(null, 0, 200);
            proveedores.Clear();
            proveedores.AddRange(proveedoresPage.Items.Where(item => item.IsActive));

            bodegas.Clear();
            bodegas.AddRange((await InventarioApiClient.GetBodegasAsync()).Where(item => item.IsActive));

            puntosEmision.Clear();
            puntosEmision.AddRange(await FacturacionApiClient.GetPuntosEmisionAsync());

            formasPago.Clear();
            formasPago.AddRange(await CatalogosApiClient.GetItemsAsync("FORMA_PAGO_SRI", true));
            request.FormaPago = formasPago.FirstOrDefault()?.Codigo ?? "01";

            if (IsLiquidacion && puntosEmision.Count > 0)
            {
                var punto = puntosEmision.FirstOrDefault(current => current.IsDefault) ?? puntosEmision[0];
                request.Establecimiento = punto.Establecimiento;
                request.PuntoEmision = punto.PuntoEmision;
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudieron cargar proveedores, bodegas o puntos de emision.";
        }
    }

    private async Task SearchProductsAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        isLoadingProducts = true;
        errorMessage = null;

        try
        {
            var page = await InventarioApiClient.GetProductosAsync(productSearchTerm, 0, 20, request.BodegaId == Guid.Empty ? null : request.BodegaId);
            productSearchResults.Clear();
            productSearchResults.AddRange(page.Items.Where(item => item.IsActive));
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el catalogo de productos.";
        }
        finally
        {
            isLoadingProducts = false;
        }
    }

    private void AddProduct(ProductoResponse producto)
    {
        errorMessage = null;
        successMessage = null;
        var porcentajeIva = IsNotaVenta ? 0m : producto.PorcentajeIva;
        var codigoIva = IsNotaVenta ? "0" : producto.CodigoIva;

        var existing = detailRows.FirstOrDefault(row => row.ProductoId == producto.Id);
        if (existing is not null)
        {
            existing.Cantidad += 1;
            existing.CostoUnitario = existing.CostoUnitario <= 0 ? SuggestCost(producto) : existing.CostoUnitario;
            existing.PorcentajeIva = porcentajeIva;
            existing.CodigoIva = codigoIva;
            return;
        }

        detailRows.Add(new CompraDetalleRowModel
        {
            ProductoId = producto.Id,
            ProductoCodigo = producto.Codigo,
            ProductoNombre = producto.Nombre,
            CodigoIva = codigoIva,
            PorcentajeIva = porcentajeIva,
            Cantidad = 1,
            CostoUnitario = SuggestCost(producto),
            Descuento = 0
        });
    }

    private void RemoveProduct(Guid productoId)
    {
        var row = detailRows.FirstOrDefault(item => item.ProductoId == productoId);
        if (row is not null)
        {
            detailRows.Remove(row);
        }
    }

    private async Task SaveAsync()
    {
        errorMessage = null;
        successMessage = null;

        if (!CanSave)
        {
            errorMessage = "Completa proveedor, bodega y al menos una linea antes de registrar.";
            return;
        }

        request.Detalles = detailRows.Select(row => new RegistrarCompraDetalleRequest
        {
            ProductoId = row.ProductoId,
            Cantidad = SafeValue(row.Cantidad),
            CostoUnitario = SafeValue(row.CostoUnitario),
            Descuento = SafeValue(row.Descuento)
        }).ToList();

        if (!ValidatePageModel())
        {
            return;
        }

        isSaving = true;
        request.FechaEmision = new DateTimeOffset(fechaEmisionLocal.Year, fechaEmisionLocal.Month, fechaEmisionLocal.Day, 0, 0, 0, DateTimeOffset.Now.Offset);

        try
        {
            var result = await ComprasApiClient.RegistrarAsync(request);
            if (!result.Succeeded || result.Data is null)
            {
                errorMessage = result.ErrorMessage ?? "No se pudo registrar la compra.";
                return;
            }

            successMessage = BuildSuccessMessage(result.Data);
            request = BuildEmptyRequest();
            if (isLiquidacionRoute)
            {
                request.TipoDocumentoCodigo = CompraDocumentTypes.LiquidacionCompra;
            }
            fechaEmisionLocal = DateTime.Today;
            detailRows.Clear();
            productSearchTerm = string.Empty;
            if (IsLiquidacion && puntosEmision.Count > 0)
            {
                var punto = puntosEmision.FirstOrDefault(current => current.IsDefault) ?? puntosEmision[0];
                request.Establecimiento = punto.Establecimiento;
                request.PuntoEmision = punto.PuntoEmision;
            }
            await LoadProductsAsync();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo registrar la compra.";
        }
        finally
        {
            isSaving = false;
        }
    }

    private bool ValidatePageModel()
    {
        if (!IsLiquidacion &&
            (string.IsNullOrWhiteSpace(request.NumeroComprobante) || request.NumeroComprobante.Count(character => character == '-') != 2))
        {
            errorMessage = "El numero de comprobante debe usar el formato 001-001-000000001.";
            return false;
        }

        if (IsLiquidacion &&
            (string.IsNullOrWhiteSpace(request.Establecimiento) || string.IsNullOrWhiteSpace(request.PuntoEmision)))
        {
            errorMessage = "Selecciona el establecimiento y punto de emision para la liquidacion.";
            return false;
        }

        foreach (var row in detailRows)
        {
            if (SafeValue(row.Cantidad) <= 0 || SafeValue(row.CostoUnitario) <= 0)
            {
                errorMessage = $"Revisa cantidad y costo del producto {row.ProductoNombre}.";
                return false;
            }

            if (SafeValue(row.Descuento) > Math.Round(SafeValue(row.Cantidad) * SafeValue(row.CostoUnitario), 2, MidpointRounding.AwayFromZero))
            {
                errorMessage = $"El descuento de {row.ProductoNombre} supera el subtotal de la linea.";
                return false;
            }
        }

        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            errorMessage = validationResults.FirstOrDefault()?.ErrorMessage ?? "La compra contiene datos invalidos.";
            return false;
        }

        return true;
    }

    private async Task OnBodegaChangedAsync()
    {
        await LoadProductsAsync();
    }

    private Task OnProveedorChangedAsync()
    {
        var proveedor = proveedores.FirstOrDefault(current => current.Id == request.ProveedorId);
        if (proveedor is null || !proveedor.PermiteCredito)
        {
            request.DiasCredito = 0;
            return Task.CompletedTask;
        }

        request.DiasCredito = Math.Max(0, proveedor.DiasCredito);
        return Task.CompletedTask;
    }

    private void OnNumeroComprobanteChanged(string? value)
    {
        request.NumeroComprobante = FormatNumeroComprobante(value);
    }

    private Task OnTipoDocumentoChangedAfterAsync()
    {
        var normalized = request.TipoDocumentoCodigo?.Trim() ?? CompraDocumentTypes.FacturaProveedor;
        if (!CompraDocumentTypes.IsSupported(normalized))
        {
            normalized = CompraDocumentTypes.FacturaProveedor;
        }

        request.TipoDocumentoCodigo = isLiquidacionRoute ? CompraDocumentTypes.LiquidacionCompra : normalized;
        detailRows.Clear();

        if (IsNotaVenta)
        {
            request.ClaveAccesoProveedor = null;
        }

        if (IsLiquidacion)
        {
            request.NumeroComprobante = null;
            var punto = puntosEmision.FirstOrDefault(current =>
                current.Establecimiento == request.Establecimiento &&
                current.PuntoEmision == request.PuntoEmision)
                ?? puntosEmision.FirstOrDefault(current => current.IsDefault)
                ?? puntosEmision.FirstOrDefault();

            if (punto is not null)
            {
                request.Establecimiento = punto.Establecimiento;
                request.PuntoEmision = punto.PuntoEmision;
            }
        }
        else
        {
            request.Establecimiento = null;
            request.PuntoEmision = null;
            request.NumeroComprobante = string.Empty;
        }

        return Task.CompletedTask;
    }

    private decimal CalculateSubtotalByRate(decimal rate)
    {
        return Math.Round(
            detailRows.Where(row => row.PorcentajeIva == rate)
                .Sum(row => row.SubtotalSinImpuesto),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal SuggestCost(ProductoResponse producto)
    {
        return producto.CostoPromedio > 0
            ? Math.Round(producto.CostoPromedio, 6, MidpointRounding.AwayFromZero)
            : Math.Round(producto.PrecioVenta, 6, MidpointRounding.AwayFromZero);
    }

    private static decimal SafeValue(decimal value) => value < 0 ? 0 : value;

    private static RegistrarCompraRequest BuildEmptyRequest()
    {
        return new RegistrarCompraRequest
        {
            NumeroComprobante = string.Empty,
            TipoDocumentoCodigo = CompraDocumentTypes.FacturaProveedor,
            FormaPago = "01"
        };
    }

    private string BuildSuccessMessage(CompraResponse compra)
    {
        if (compra.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra)
        {
            return $"Liquidacion {compra.NumeroComprobante} registrada por {compra.ImporteTotal:0.00}. Estado fiscal: {compra.EstadoSri ?? "PENDIENTE"}.";
        }

        return $"{compra.TipoDocumentoNombre} {compra.NumeroComprobante} registrada correctamente por {compra.ImporteTotal:0.00}.";
    }

    private static string FormatNumeroComprobante(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).Take(15).ToArray());
        if (digits.Length == 0)
        {
            return string.Empty;
        }

        var first = digits[..Math.Min(3, digits.Length)];
        if (digits.Length <= 3)
        {
            return first;
        }

        var secondLength = Math.Min(3, digits.Length - 3);
        var second = digits.Substring(3, secondLength);
        if (digits.Length <= 6)
        {
            return $"{first}-{second}";
        }

        var third = digits[6..];
        return $"{first}-{second}-{third}";
    }

    private static string BuildBodegaLabel(BodegaResponse bodega)
    {
        return string.IsNullOrWhiteSpace(bodega.Direccion)
            ? bodega.Nombre
            : $"{bodega.Nombre} - {bodega.Direccion}";
    }

    private sealed class CompraDetalleRowModel
    {
        public Guid ProductoId { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public string CodigoIva { get; set; } = string.Empty;
        public decimal PorcentajeIva { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal SubtotalSinImpuesto => Math.Round(Math.Max(0, (Cantidad * CostoUnitario) - Descuento), 2, MidpointRounding.AwayFromZero);
        public decimal TotalImpuesto => Math.Round(SubtotalSinImpuesto * (PorcentajeIva / 100m), 2, MidpointRounding.AwayFromZero);
        public decimal TotalLinea => SubtotalSinImpuesto + TotalImpuesto;
    }
}
