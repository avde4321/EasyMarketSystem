using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TestDeIa.Client.Services.Catalogos;
using TestDeIa.Client.Services.Compras;
using TestDeIa.Client.Services.Facturacion;
using TestDeIa.Client.Services.Inventario;
using TestDeIa.Client.Services;
using TestDeIa.Shared.Compras;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Responses.Catalogos;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Responses.Facturacion;
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

    [Inject]
    private PopupNotificationService PopupNotificationService { get; set; } = default!;

    private static readonly string[] WorkflowSteps = ["Configuración", "Productos", "Detalle"];

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
    private bool isAnalyzingInvoice;
    private bool isLiquidacionRoute;
    private bool isWorkflowModalOpen;
    private int currentStep = 1;
    private string? errorMessage;
    private string? successMessage;
    private DateTime fechaEmisionLocal = DateTime.Today;
    private string nonInventoryName = string.Empty;
    private string nonInventoryCategory = "1.2.01.01";
    private string nonInventoryLocation = string.Empty;
    private decimal nonInventoryQuantity = 1m;
    private decimal nonInventoryCost;
    private decimal nonInventoryDiscount;
    private FacturaProveedorAnalisisResponse? invoiceAnalysis;

    private bool IsLiquidacion => request.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra;
    private bool IsNotaVenta => request.TipoDocumentoCodigo == CompraDocumentTypes.NotaVentaRimpe;
    private bool IsInventarioCompra => request.NaturalezaCompra == NaturalezaCompra.MercaderiaInventario;
    private bool IsActivoFijoCompra => request.NaturalezaCompra == NaturalezaCompra.ActivoFijo;
    private bool IsGastoServicioCompra => request.NaturalezaCompra == NaturalezaCompra.GastoServicio;
    private decimal SubtotalIva0 => CalculateSubtotalByRate(0m);
    private decimal SubtotalIva5 => IsNotaVenta ? 0 : CalculateSubtotalByRate(5m);
    private decimal SubtotalIva8 => IsNotaVenta ? 0 : CalculateSubtotalByRate(8m);
    private decimal SubtotalIva15 => IsNotaVenta ? 0 : CalculateSubtotalByRate(15m);
    private decimal TotalDescuento => Math.Round(detailRows.Sum(row => SafeValue(row.Descuento)), 2, MidpointRounding.AwayFromZero);
    private decimal TotalImpuestos => Math.Round(detailRows.Sum(row => row.TotalImpuesto), 2, MidpointRounding.AwayFromZero);
    private decimal ImporteTotal => Math.Round(detailRows.Sum(row => row.TotalLinea), 2, MidpointRounding.AwayFromZero);
    private bool HasSelectedBodega => request.BodegaId != Guid.Empty;
    private BodegaResponse? selectedBodega => bodegas.FirstOrDefault(current => current.Id == request.BodegaId);
    private bool CanSave => detailRows.Count > 0 && request.ProveedorId != Guid.Empty && request.BodegaId != Guid.Empty && !isLoadingProducts;
    private int CurrentStep => currentStep;

    private string PageTitleText => IsLiquidacion ? "Liquidaciones de compra" : "Registro de compras";
    private string PrimaryActionText => IsLiquidacion ? "Emitir liquidación" : "Registrar compra";
    private string PageDescription => IsLiquidacion
        ? "Emisión de liquidaciones con impacto directo en bodega, Kardex y costo promedio."
        : "Ingreso operativo de documentos de proveedor con impacto directo en bodega, Kardex y costo promedio.";
    private string SummaryDescription => "Trabaja con un asistente por pasos. Puedes regresar entre pantallas sin perder la configuración ni los productos agregados.";
    private string FlowName => IsLiquidacion ? "Liquidación de compra" : IsNotaVenta ? "Nota de venta proveedor" : "Factura proveedor";
    private string FlowSummary => IsLiquidacion
        ? "Emite el documento interno y deja la liquidación en cola para la firma electrónica."
        : "Registra el documento del proveedor e incrementa inventario en la bodega elegida.";
    private string SelectedProveedorLabel => proveedores.FirstOrDefault(current => current.Id == request.ProveedorId)?.NombreCompleto ?? "Pendiente de selección";
    private string SelectedProveedorSupport => proveedores.FirstOrDefault(current => current.Id == request.ProveedorId)?.Identificacion ?? "Define el proveedor en el paso 1";
    private string SelectedBodegaLabel => selectedBodega?.Nombre ?? "Pendiente de selección";
    private string SelectedBodegaSupport => selectedBodega?.Direccion ?? "La bodega define dónde subirá el stock";
    private string SelectedDocumentLabel => IsLiquidacion ? BuildLiquidacionSeriesLabel() : (string.IsNullOrWhiteSpace(request.NumeroComprobante) ? "Pendiente de selección" : request.NumeroComprobante!);
    private string SelectedDocumentSupport => IsLiquidacion ? "Serie interna de liquidación" : "Número del documento del proveedor";

    private bool CanApplyInvoiceAnalysis => invoiceAnalysis is { Succeeded: true };

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
        isWorkflowModalOpen = IsLiquidacion;
    }

    private void NavigateToXmlImport()
    {
        NavigationManager.NavigateTo("/compras/xml-sri");
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

            if (request.BodegaId == Guid.Empty && bodegas.Count > 0)
            {
                request.BodegaId = SelectPreferredBodegaId();
            }

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
            errorMessage = "No se pudieron cargar proveedores, bodegas o puntos de emisión.";
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
            productSearchResults.Clear();

            if (!HasSelectedBodega)
            {
                return;
            }

            if (!IsInventarioCompra)
            {
                return;
            }

            var page = await InventarioApiClient.GetProductosAsync(productSearchTerm, 0, 20, request.BodegaId);
            productSearchResults.AddRange(page.Items.Where(item => item.IsActive));
            RefreshDetailRowsFromSearchResults();
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo cargar el catálogo de productos.";
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

        if (!HasSelectedBodega)
        {
            errorMessage = "Selecciona primero la bodega destino para agregar productos con el stock correcto.";
            return;
        }

        if (!IsInventarioCompra)
        {
            errorMessage = "Para activos fijos o gastos usa el formulario de detalle de esta pantalla.";
            return;
        }

        var porcentajeIva = IsNotaVenta ? 0m : producto.PorcentajeIva;
        var codigoIva = IsNotaVenta ? "0" : producto.CodigoIva;

        var existing = detailRows.FirstOrDefault(row => row.ProductoId == producto.Id);
        if (existing is not null)
        {
            existing.Cantidad += 1;
            existing.CostoUnitario = existing.CostoUnitario <= 0 ? SuggestCost(producto) : existing.CostoUnitario;
            existing.PorcentajeIva = porcentajeIva;
            existing.CodigoIva = codigoIva;
            existing.ControlaStock = producto.ControlaStock;
            existing.StockActualBodega = producto.StockActual;
            currentStep = 3;
            return;
        }

        detailRows.Add(new CompraDetalleRowModel
        {
            ProductoId = producto.Id,
            NaturalezaCompra = NaturalezaCompra.MercaderiaInventario,
            ProductoCodigo = producto.Codigo,
            ProductoNombre = producto.Nombre,
            CodigoIva = codigoIva,
            PorcentajeIva = porcentajeIva,
            Cantidad = 1,
            CostoUnitario = SuggestCost(producto),
            Descuento = 0,
            ControlaStock = producto.ControlaStock,
            StockActualBodega = producto.StockActual
        });

        currentStep = 3;
    }

    private void AddNonInventoryLine()
    {
        errorMessage = null;
        successMessage = null;

        if (IsInventarioCompra)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nonInventoryName))
        {
            errorMessage = IsActivoFijoCompra
                ? "Ingresa el nombre del activo fijo."
                : "Ingresa el concepto del gasto o servicio.";
            return;
        }

        if (IsActivoFijoCompra && string.IsNullOrWhiteSpace(nonInventoryCategory))
        {
            errorMessage = "Selecciona la categoría SRI del activo fijo.";
            return;
        }

        if (SafeValue(nonInventoryQuantity) <= 0 || SafeValue(nonInventoryCost) <= 0)
        {
            errorMessage = "Revisa cantidad y costo antes de agregar la línea.";
            return;
        }

        var porcentajeIva = IsNotaVenta ? 0m : 15m;
        var codigoIva = IsNotaVenta ? "0" : "4";
        detailRows.Add(new CompraDetalleRowModel
        {
            ProductoId = null,
            NaturalezaCompra = request.NaturalezaCompra,
            ProductoCodigo = IsActivoFijoCompra ? "ACT-FIJO" : "GASTO",
            ProductoNombre = nonInventoryName.Trim(),
            CodigoIva = codigoIva,
            PorcentajeIva = porcentajeIva,
            Cantidad = SafeValue(nonInventoryQuantity),
            CostoUnitario = SafeValue(nonInventoryCost),
            Descuento = SafeValue(nonInventoryDiscount),
            ControlaStock = false,
            NombreActivo = nonInventoryName.Trim(),
            CategoriaSriActivo = nonInventoryCategory,
            SerieUbicacionActivo = string.IsNullOrWhiteSpace(nonInventoryLocation) ? null : nonInventoryLocation.Trim()
        });

        ResetNonInventoryDraft();
        currentStep = 3;
    }

    private void RemoveProduct(CompraDetalleRowModel row)
    {
        detailRows.Remove(row);
    }

    private async Task SaveAsync()
    {
        errorMessage = null;
        successMessage = null;

        if (!CanSave)
        {
            errorMessage = "Completa proveedor, bodega y al menos una línea antes de registrar.";
            return;
        }

        request.Detalles = detailRows.Select(row => new RegistrarCompraDetalleRequest
        {
            ProductoId = row.ProductoId,
            NaturalezaCompra = row.NaturalezaCompra,
            NombreActivo = row.NombreActivo,
            CategoriaSriActivo = row.CategoriaSriActivo,
            SerieUbicacionActivo = row.SerieUbicacionActivo,
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
            await this.PopupNotificationService.ShowSuccessAsync(successMessage);
            request = BuildEmptyRequest();
            if (isLiquidacionRoute)
            {
                request.TipoDocumentoCodigo = CompraDocumentTypes.LiquidacionCompra;
            }

            request.BodegaId = SelectPreferredBodegaId();
            fechaEmisionLocal = DateTime.Today;
            detailRows.Clear();
            productSearchTerm = string.Empty;
            currentStep = 1;

            if (IsLiquidacion && puntosEmision.Count > 0)
            {
                var punto = puntosEmision.FirstOrDefault(current => current.IsDefault) ?? puntosEmision[0];
                request.Establecimiento = punto.Establecimiento;
                request.PuntoEmision = punto.PuntoEmision;
            }

            await LoadProductsAsync();
            isWorkflowModalOpen = false;
        }
        catch (HttpRequestException)
        {
            errorMessage = "No se pudo registrar la compra.";
            await this.PopupNotificationService.ShowErrorAsync(errorMessage);
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
            errorMessage = "El número de comprobante debe usar el formato 001-001-000000001.";
            return false;
        }

        if (IsLiquidacion &&
            (string.IsNullOrWhiteSpace(request.Establecimiento) || string.IsNullOrWhiteSpace(request.PuntoEmision)))
        {
            errorMessage = "Selecciona el establecimiento y punto de emisión para la liquidación.";
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
                errorMessage = $"El descuento de {row.ProductoNombre} supera el subtotal de la línea.";
                return false;
            }

            if (row.NaturalezaCompra == NaturalezaCompra.ActivoFijo &&
                (string.IsNullOrWhiteSpace(row.NombreActivo) || string.IsNullOrWhiteSpace(row.CategoriaSriActivo)))
            {
                errorMessage = "Cada activo fijo debe tener nombre y categoría SRI.";
                return false;
            }
        }

        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            errorMessage = validationResults.FirstOrDefault()?.ErrorMessage ?? "La compra contiene datos inválidos.";
            return false;
        }

        return true;
    }

    private async Task OnBodegaChangedAsync()
    {
        errorMessage = null;
        successMessage = null;
        detailRows.Clear();
        ResetNonInventoryDraft();
        await LoadProductsAsync();
    }

    private Task OnProveedorChangedAsync()
    {
        var proveedor = proveedores.FirstOrDefault(current => current.Id == request.ProveedorId);
        request.DiasCredito = proveedor is not null && proveedor.PermiteCredito
            ? Math.Max(0, proveedor.DiasCredito)
            : 0;

        return Task.CompletedTask;
    }

    private Task OnNumeroComprobanteBoundAsync()
    {
        request.NumeroComprobante = FormatNumeroComprobante(request.NumeroComprobante);
        return Task.CompletedTask;
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
        ResetNonInventoryDraft();

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

    private Task OnFormaPagoCompraChangedAsync()
    {
        request.FormaPago = request.FormaPagoCompra switch
        {
            FormaPagoCompra.TransferenciaBancaria => "20",
            FormaPagoCompra.Cheque => "20",
            FormaPagoCompra.TarjetaCredito => "19",
            FormaPagoCompra.CreditoProveedores => "20",
            _ => "01"
        };

        if (request.FormaPagoCompra == FormaPagoCompra.CreditoProveedores && request.DiasCredito <= 0)
        {
            request.DiasCredito = 30;
        }

        return Task.CompletedTask;
    }

    private async Task OnNaturalezaCompraChangedAsync()
    {
        errorMessage = null;
        successMessage = null;
        detailRows.Clear();
        productSearchResults.Clear();
        productSearchTerm = string.Empty;
        ResetNonInventoryDraft();
        await LoadProductsAsync();
    }

    private void OpenWorkflowModal()
    {
        isWorkflowModalOpen = true;
        errorMessage = null;
        successMessage = null;
    }

    private void CloseWorkflowModal()
    {
        isWorkflowModalOpen = false;
    }

    private void GoToStep(int step)
    {
        if (step < 1 || step > WorkflowSteps.Length)
        {
            return;
        }

        if (step > currentStep + 1 && detailRows.Count == 0)
        {
            return;
        }

        currentStep = step;
    }

    private void GoPreviousStep()
    {
        if (currentStep > 1)
        {
            currentStep--;
        }
    }

    private void GoNextStep()
    {
        if (currentStep == 1 && !HasSelectedBodega)
        {
            errorMessage = "Selecciona una bodega antes de continuar.";
            return;
        }

        if (currentStep == 2 && detailRows.Count == 0)
        {
            errorMessage = "Agrega al menos un producto antes de continuar al detalle.";
            return;
        }

        if (currentStep < WorkflowSteps.Length)
        {
            currentStep++;
        }
    }

    private async Task AnalyzeInvoiceFileAsync(InputFileChangeEventArgs args)
    {
        errorMessage = null;
        successMessage = null;
        invoiceAnalysis = null;

        var file = args.File;
        if (file is null)
        {
            return;
        }

        isAnalyzingInvoice = true;
        try
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 10_000_000);
            var result = await ComprasApiClient.AnalizarFacturaProveedorAsync(stream, file.Name, file.ContentType);
            if (!result.Succeeded || result.Data is null)
            {
                errorMessage = result.ErrorMessage ?? "No se pudo analizar la factura del proveedor.";
                await PopupNotificationService.ShowErrorAsync(errorMessage);
                return;
            }

            invoiceAnalysis = result.Data;
            isWorkflowModalOpen = true;
            currentStep = 1;
            if (!invoiceAnalysis.Succeeded)
            {
                await PopupNotificationService.ShowInfoAsync(invoiceAnalysis.Message);
            }
        }
        catch (IOException)
        {
            errorMessage = "No se pudo leer el archivo seleccionado.";
            await PopupNotificationService.ShowErrorAsync(errorMessage);
        }
        finally
        {
            isAnalyzingInvoice = false;
        }
    }

    private void ClearInvoiceAnalysis()
    {
        invoiceAnalysis = null;
    }

    private async Task ApplyInvoiceAnalysisAsync()
    {
        if (invoiceAnalysis is not { Succeeded: true })
        {
            return;
        }

        request.TipoDocumentoCodigo = CompraDocumentTypes.FacturaProveedor;
        request.TipoComprobanteSRI = string.IsNullOrWhiteSpace(invoiceAnalysis.DocumentoTipo) ? CompraDocumentTypes.FacturaProveedor : invoiceAnalysis.DocumentoTipo;
        request.NumeroComprobante = FormatNumeroComprobante(invoiceAnalysis.NumeroComprobante);
        request.ClaveAccesoProveedor = invoiceAnalysis.ClaveAcceso;
        request.NumeroAutorizacion = invoiceAnalysis.NumeroAutorizacion;
        request.SustentoTributarioSRI = "01";

        if (invoiceAnalysis.FechaEmision.HasValue)
        {
            fechaEmisionLocal = invoiceAnalysis.FechaEmision.Value.Date;
        }

        var proveedor = proveedores.FirstOrDefault(current =>
            !string.IsNullOrWhiteSpace(invoiceAnalysis.ProveedorRuc) &&
            string.Equals(current.Identificacion, invoiceAnalysis.ProveedorRuc, StringComparison.OrdinalIgnoreCase));
        if (proveedor is not null)
        {
            request.ProveedorId = proveedor.Id;
            await OnProveedorChangedAsync();
        }

        detailRows.Clear();
        foreach (var detalle in invoiceAnalysis.Detalles)
        {
            detailRows.Add(new CompraDetalleRowModel
            {
                ProductoId = null,
                NaturalezaCompra = NaturalezaCompra.GastoServicio,
                ProductoCodigo = string.IsNullOrWhiteSpace(detalle.CodigoPrincipal) ? "FACT-PROV" : detalle.CodigoPrincipal,
                ProductoNombre = detalle.Descripcion,
                CodigoIva = detalle.TarifaIva == 0 ? "0" : "4",
                PorcentajeIva = detalle.TarifaIva,
                Cantidad = detalle.Cantidad <= 0 ? 1 : detalle.Cantidad,
                CostoUnitario = detalle.PrecioUnitario > 0
                    ? detalle.PrecioUnitario
                    : Math.Round(detalle.Subtotal / Math.Max(1, detalle.Cantidad), 6, MidpointRounding.AwayFromZero),
                Descuento = detalle.Descuento,
                ControlaStock = false,
                NombreActivo = detalle.Descripcion,
                CategoriaSriActivo = "5.1.01.01"
            });
        }

        if (detailRows.Count > 0)
        {
            request.NaturalezaCompra = NaturalezaCompra.GastoServicio;
        }

        currentStep = detailRows.Count > 0 ? 3 : 1;
        successMessage = "Datos de factura aplicados al formulario. Revisa la clasificación y productos antes de guardar.";
        await PopupNotificationService.ShowSuccessAsync(successMessage);
    }

    private decimal CalculateSubtotalByRate(decimal rate)
    {
        return Math.Round(
            detailRows.Where(row => row.PorcentajeIva == rate)
                .Sum(row => row.SubtotalSinImpuesto),
            2,
            MidpointRounding.AwayFromZero);
    }

    private void RefreshDetailRowsFromSearchResults()
    {
        if (detailRows.Count == 0)
        {
            return;
        }

        foreach (var row in detailRows)
        {
            var producto = productSearchResults.FirstOrDefault(item => item.Id == row.ProductoId);
            if (producto is null)
            {
                continue;
            }

            row.ControlaStock = producto.ControlaStock;
            row.StockActualBodega = producto.StockActual;
            row.CostoUnitario = row.CostoUnitario <= 0 ? SuggestCost(producto) : row.CostoUnitario;
        }
    }

    private Guid SelectPreferredBodegaId()
    {
        var preferred = bodegas
            .OrderByDescending(bodega => string.Equals(bodega.Nombre, "Principal", StringComparison.OrdinalIgnoreCase))
            .ThenBy(bodega => bodega.Nombre)
            .FirstOrDefault();

        return preferred?.Id ?? Guid.Empty;
    }

    private string BuildLiquidacionSeriesLabel()
    {
        if (string.IsNullOrWhiteSpace(request.Establecimiento) || string.IsNullOrWhiteSpace(request.PuntoEmision))
        {
            return "Pendiente de selección";
        }

        return $"{request.Establecimiento}-{request.PuntoEmision}";
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

    private void ResetNonInventoryDraft()
    {
        nonInventoryName = string.Empty;
        nonInventoryCategory = IsGastoServicioCompra ? "5.1.01.01" : "1.2.01.01";
        nonInventoryLocation = string.Empty;
        nonInventoryQuantity = 1m;
        nonInventoryCost = 0m;
        nonInventoryDiscount = 0m;
    }

    private string BuildSuccessMessage(CompraResponse compra)
    {
        var bodegaNombre = selectedBodega?.Nombre ?? "la bodega seleccionada";
        var totalItemsInventariables = detailRows.Count(row => row.ControlaStock);

        if (compra.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra)
        {
            return totalItemsInventariables > 0
                ? $"Liquidación {compra.NumeroComprobante} registrada por {compra.ImporteTotal:0.00}. Estado fiscal: {compra.EstadoSri ?? "PENDIENTE"}. El inventario se incrementó en {bodegaNombre}."
                : $"Liquidación {compra.NumeroComprobante} registrada por {compra.ImporteTotal:0.00}. Estado fiscal: {compra.EstadoSri ?? "PENDIENTE"}.";
        }

        return totalItemsInventariables > 0
            ? $"{compra.TipoDocumentoNombre} {compra.NumeroComprobante} registrada correctamente por {compra.ImporteTotal:0.00}. El stock ingresó en {bodegaNombre}."
            : $"{compra.TipoDocumentoNombre} {compra.NumeroComprobante} registrada correctamente por {compra.ImporteTotal:0.00}. Clasificación: {compra.NaturalezaCompra}.";
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
        public Guid? ProductoId { get; set; }
        public NaturalezaCompra NaturalezaCompra { get; set; } = NaturalezaCompra.MercaderiaInventario;
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public string? NombreActivo { get; set; }
        public string? CategoriaSriActivo { get; set; }
        public string? SerieUbicacionActivo { get; set; }
        public string CodigoIva { get; set; } = string.Empty;
        public decimal PorcentajeIva { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Descuento { get; set; }
        public bool ControlaStock { get; set; }
        public decimal StockActualBodega { get; set; }
        public decimal StockProyectado => ControlaStock ? StockActualBodega + Cantidad : 0;
        public decimal SubtotalSinImpuesto => Math.Round(Math.Max(0, (Cantidad * CostoUnitario) - Descuento), 2, MidpointRounding.AwayFromZero);
        public decimal TotalImpuesto => Math.Round(SubtotalSinImpuesto * (PorcentajeIva / 100m), 2, MidpointRounding.AwayFromZero);
        public decimal TotalLinea => SubtotalSinImpuesto + TotalImpuesto;
    }
}







