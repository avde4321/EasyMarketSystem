using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Application.Modules.Contabilidad.Ports.In;
using TestDeIa.Application.Modules.Contabilidad.Ports.Out;
using TestDeIa.Application.Modules.Empresa.Ports.Out;
using TestDeIa.Application.Modules.Inventario.Ports.Out;
using TestDeIa.Application.Modules.Security.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;
using DomainFormaPagoCompra = TestDeIa.Domain.Modules.Compras.Enums.FormaPagoCompra;
using DomainNaturalezaCompra = TestDeIa.Domain.Modules.Compras.Enums.NaturalezaCompra;
using TestDeIa.Shared.Compras;
using TestDeIa.Shared.Requests.Compras;
using TestDeIa.Shared.Requests.Contabilidad;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Application.Modules.Compras.UseCases;

public sealed class CompraUseCase : ICompraUseCase
{
    private static readonly decimal[] SupportedIvaRates = [0m, 5m, 8m, 15m];

    private readonly ICompraRepository compraRepository;
    private readonly ICuentaPorPagarRepository cuentaPorPagarRepository;
    private readonly ICompraBackgroundQueue compraBackgroundQueue;
    private readonly IProveedorRepository proveedorRepository;
    private readonly IEmpresaRepository empresaRepository;
    private readonly IInventarioRepository inventarioRepository;
    private readonly ICurrentUserAccessor currentUserAccessor;
    private readonly IContabilidadService contabilidadService;
    private readonly IContabilidadRepository contabilidadRepository;

    public CompraUseCase(
        ICompraRepository compraRepository,
        ICuentaPorPagarRepository cuentaPorPagarRepository,
        ICompraBackgroundQueue compraBackgroundQueue,
        IProveedorRepository proveedorRepository,
        IEmpresaRepository empresaRepository,
        IInventarioRepository inventarioRepository,
        ICurrentUserAccessor currentUserAccessor,
        IContabilidadService contabilidadService,
        IContabilidadRepository contabilidadRepository)
    {
        this.compraRepository = compraRepository;
        this.cuentaPorPagarRepository = cuentaPorPagarRepository;
        this.compraBackgroundQueue = compraBackgroundQueue;
        this.proveedorRepository = proveedorRepository;
        this.empresaRepository = empresaRepository;
        this.inventarioRepository = inventarioRepository;
        this.currentUserAccessor = currentUserAccessor;
        this.contabilidadService = contabilidadService;
        this.contabilidadRepository = contabilidadRepository;
    }

    public async Task<CompraResponse> RegistrarAsync(RegistrarCompraRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var proveedor = await proveedorRepository.GetByIdAsync(request.ProveedorId, cancellationToken)
            ?? throw new InvalidOperationException("El proveedor seleccionado no existe en la empresa activa.");

        if (!proveedor.IsActive)
        {
            throw new InvalidOperationException("El proveedor seleccionado esta inactivo.");
        }

        if (request.DiasCredito > 0 && !proveedor.PermiteCredito)
        {
            throw new InvalidOperationException("El proveedor seleccionado no tiene credito habilitado.");
        }

        var bodega = await inventarioRepository.GetBodegaByIdAsync(request.BodegaId, cancellationToken)
            ?? throw new InvalidOperationException("La bodega seleccionada no existe en la empresa activa.");

        if (!bodega.IsActive)
        {
            throw new InvalidOperationException("La bodega destino esta inactiva.");
        }

        var tipoDocumento = request.TipoDocumentoCodigo.Trim();
        var isLiquidacion = tipoDocumento == CompraDocumentTypes.LiquidacionCompra;
        var isNotaVenta = tipoDocumento == CompraDocumentTypes.NotaVentaRimpe;

        var naturalezaCompra = ToDomainNaturaleza(request.NaturalezaCompra);
        request.TipoComprobanteSRI = string.IsNullOrWhiteSpace(request.TipoComprobanteSRI)
            ? tipoDocumento
            : request.TipoComprobanteSRI.Trim();

        var detalles = new List<CompraDetalle>(request.Detalles.Count);
        foreach (var detalleRequest in request.Detalles)
        {
            var detalleNaturaleza = ToDomainNaturaleza(detalleRequest.NaturalezaCompra);
            if (detalleNaturaleza != naturalezaCompra)
            {
                detalleNaturaleza = naturalezaCompra;
            }

            Guid? productoId = null;
            string productoCodigo;
            string productoNombre;
            string codigoIva;
            decimal porcentajeIva;

            if (detalleNaturaleza == DomainNaturalezaCompra.MercaderiaInventario)
            {
                if (!detalleRequest.ProductoId.HasValue || detalleRequest.ProductoId.Value == Guid.Empty)
                {
                    throw new InvalidOperationException("Todas las lineas de inventario deben tener un producto valido.");
                }

                var producto = await inventarioRepository.GetProductoByIdAsync(detalleRequest.ProductoId.Value, cancellationToken)
                    ?? throw new InvalidOperationException("Uno de los productos seleccionados no existe.");

                if (!producto.IsActive)
                {
                    throw new InvalidOperationException($"El producto {producto.Nombre} esta inactivo.");
                }

                if (!SupportedIvaRates.Contains(producto.PorcentajeIva))
                {
                    throw new InvalidOperationException($"El producto {producto.Nombre} tiene una tarifa IVA no soportada para compras.");
                }

                productoId = producto.Id;
                productoCodigo = producto.Codigo;
                productoNombre = producto.Nombre;
                porcentajeIva = isNotaVenta ? 0m : producto.PorcentajeIva;
                codigoIva = isNotaVenta ? "0" : producto.CodigoIva;
            }
            else
            {
                productoCodigo = detalleNaturaleza == DomainNaturalezaCompra.ActivoFijo ? "ACT-FIJO" : "GASTO";
                productoNombre = NormalizeOptional(detalleRequest.NombreActivo) ?? (detalleNaturaleza == DomainNaturalezaCompra.ActivoFijo ? "Activo fijo" : "Gasto / servicio");
                porcentajeIva = isNotaVenta ? 0m : 15m;
                codigoIva = isNotaVenta ? "0" : "4";
            }

            var subtotalSinImpuesto = Math.Round((detalleRequest.Cantidad * detalleRequest.CostoUnitario) - detalleRequest.Descuento, 2, MidpointRounding.AwayFromZero);
            if (subtotalSinImpuesto < 0)
            {
                throw new InvalidOperationException($"El descuento de {productoNombre} no puede superar el subtotal de la linea.");
            }

            detalles.Add(new CompraDetalle(
                Guid.NewGuid(),
                Guid.Empty,
                productoId,
                productoCodigo,
                productoNombre,
                detalleNaturaleza,
                NormalizeOptional(detalleRequest.NombreActivo),
                NormalizeOptional(detalleRequest.CategoriaSriActivo),
                NormalizeOptional(detalleRequest.SerieUbicacionActivo),
                codigoIva,
                porcentajeIva,
                detalleRequest.Cantidad,
                detalleRequest.CostoUnitario,
                detalleRequest.Descuento,
                subtotalSinImpuesto));
        }

        var (establecimiento, puntoEmision, secuencial) = await ResolveDocumentSeriesAsync(request, isLiquidacion, cancellationToken);
        var formaPagoCompra = ToDomainFormaPago(request.FormaPagoCompra);
        request.FormaPago = ResolveSriFormaPago(request.FormaPagoCompra, request.FormaPago);
        var formaPagoSri = SriCatalogCodes.NormalizeFormaPagoCode(request.FormaPago)
            ?? throw new InvalidOperationException("La forma de pago seleccionada no esta mapeada a un codigo SRI valido.");
        var importeTotal = Math.Round(detalles.Sum(detalle => detalle.TotalLinea), 2, MidpointRounding.AwayFromZero);
        var requiereBancarizacion = importeTotal >= 1000m;
        if (requiereBancarizacion && formaPagoCompra == DomainFormaPagoCompra.ContadoEfectivo)
        {
            throw new InvalidOperationException("La compra supera USD 1,000.00 y requiere bancarizacion LRTI. Selecciona transferencia, cheque, tarjeta o credito proveedores.");
        }

        if (formaPagoCompra == DomainFormaPagoCompra.CreditoProveedores && request.DiasCredito <= 0)
        {
            request.DiasCredito = Math.Max(1, proveedor.DiasCredito);
        }

        var compraId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var compra = new Compra(
            compraId,
            currentUserAccessor.GetRequiredEmpresaId(),
            proveedor.Id,
            request.BodegaId,
            naturalezaCompra,
            tipoDocumento,
            request.TipoComprobanteSRI,
            request.SustentoTributarioSRI.Trim(),
            establecimiento,
            puntoEmision,
            secuencial,
            NormalizeOptional(request.ClaveAccesoProveedor),
            null,
            NormalizeOptional(request.NumeroAutorizacion),
            isLiquidacion ? Domain.Modules.Facturacion.Entities.FacturaEstado.PENDIENTE : null,
            isLiquidacion ? "Liquidacion registrada y en cola para firma electronica." : null,
            formaPagoSri,
            formaPagoCompra,
            requiereBancarizacion,
            NormalizeOptional(request.Observacion),
            null,
            null,
            null,
            null,
            0,
            null,
            request.FechaEmision,
            RoundSubtotal(detalles, 0m),
            RoundSubtotal(detalles, 5m),
            RoundSubtotal(detalles, 8m),
            RoundSubtotal(detalles, 15m),
            Math.Round(detalles.Sum(detalle => detalle.Descuento), 2, MidpointRounding.AwayFromZero),
            Math.Round(detalles.Sum(detalle => detalle.TotalImpuesto), 2, MidpointRounding.AwayFromZero),
            importeTotal,
            EstadoCompra.Registrada,
            now,
            currentUserAccessor.GetRequiredUserId(),
            null,
            null,
            detalles.Select(detalle => new CompraDetalle(
                detalle.Id,
                compraId,
                detalle.ProductoId,
                detalle.ProductoCodigo,
                detalle.ProductoNombre,
                detalle.NaturalezaCompra,
                detalle.NombreActivo,
                detalle.CategoriaSriActivo,
                detalle.SerieUbicacionActivo,
                detalle.CodigoIva,
                detalle.PorcentajeIva,
                detalle.Cantidad,
                detalle.CostoUnitario,
                detalle.Descuento,
                detalle.CostoTotalSinImpuesto)).ToArray());

        var persisted = await compraRepository.CreateAsync(compra, cancellationToken);
        if (request.DiasCredito > 0)
        {
            var cuenta = new CuentaPorPagar(
                Guid.NewGuid(),
                persisted.EmpresaId,
                persisted.Id,
                proveedor.Id,
                proveedor.Identificacion,
                proveedor.NombreCompleto,
                persisted.NumeroComprobante,
                persisted.FechaEmision,
                persisted.FechaEmision.AddDays(request.DiasCredito),
                persisted.ImporteTotal,
                persisted.ImporteTotal,
                EstadoDeuda.Pendiente,
                now,
                currentUserAccessor.GetRequiredUserId(),
                null,
                null,
                Array.Empty<PagoCxP>());

            await cuentaPorPagarRepository.CreateAsync(cuenta, cancellationToken);
        }

        if (persisted.TipoDocumentoCodigo == CompraDocumentTypes.LiquidacionCompra &&
            persisted.EstadoSri is not Domain.Modules.Facturacion.Entities.FacturaEstado.AUTORIZADO)
        {
            compraBackgroundQueue.Enqueue(persisted.Id);
        }

        await contabilidadService.GenerarAsientoDesdeOrigenAsync(persisted.Id, "Compras", cancellationToken);

        return MapToResponse(persisted);
    }

    private static void ValidateRequest(RegistrarCompraRequest request)
    {
        if (request.ProveedorId == Guid.Empty)
        {
            throw new InvalidOperationException("El proveedor es obligatorio.");
        }

        if (request.BodegaId == Guid.Empty)
        {
            throw new InvalidOperationException("La bodega destino es obligatoria.");
        }

        if (request.Detalles.Count == 0)
        {
            throw new InvalidOperationException("Debes registrar al menos un producto en la compra.");
        }

        var tipoDocumento = request.TipoDocumentoCodigo.Trim();
        if (!CompraDocumentTypes.IsSupported(tipoDocumento))
        {
            throw new InvalidOperationException("El tipo de documento seleccionado no esta soportado.");
        }

        if (!Enum.IsDefined(request.NaturalezaCompra))
        {
            throw new InvalidOperationException("La naturaleza fiscal de la compra no es valida.");
        }

        if (string.IsNullOrWhiteSpace(request.SustentoTributarioSRI))
        {
            throw new InvalidOperationException("El sustento tributario SRI es obligatorio.");
        }

        if (tipoDocumento == CompraDocumentTypes.FacturaProveedor &&
            string.IsNullOrWhiteSpace(request.ClaveAccesoProveedor))
        {
            throw new InvalidOperationException("La clave de acceso del proveedor es obligatoria para factura de proveedor.");
        }

        if (tipoDocumento != CompraDocumentTypes.LiquidacionCompra &&
            string.IsNullOrWhiteSpace(request.NumeroComprobante))
        {
            throw new InvalidOperationException("El numero de comprobante es obligatorio.");
        }

        foreach (var detalle in request.Detalles)
        {
            if (request.NaturalezaCompra == NaturalezaCompra.MercaderiaInventario &&
                (!detalle.ProductoId.HasValue || detalle.ProductoId.Value == Guid.Empty))
            {
                throw new InvalidOperationException("Todos los productos de la compra deben ser validos.");
            }

            if (request.NaturalezaCompra == NaturalezaCompra.ActivoFijo &&
                (string.IsNullOrWhiteSpace(detalle.NombreActivo) || string.IsNullOrWhiteSpace(detalle.CategoriaSriActivo)))
            {
                throw new InvalidOperationException("El activo fijo requiere nombre del bien y categoria SRI.");
            }

            if (detalle.Cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad de cada linea debe ser mayor a cero.");
            }

            if (detalle.CostoUnitario <= 0)
            {
                throw new InvalidOperationException("El costo unitario de cada linea debe ser mayor a cero.");
            }

            if (detalle.Descuento < 0)
            {
                throw new InvalidOperationException("El descuento por linea no puede ser negativo.");
            }
        }
    }

    private async Task<(string Establecimiento, string PuntoEmision, string Secuencial)> ResolveDocumentSeriesAsync(
        RegistrarCompraRequest request,
        bool isLiquidacion,
        CancellationToken cancellationToken)
    {
        if (!isLiquidacion)
        {
            return ParseNumeroComprobante(request.NumeroComprobante);
        }

        if (string.IsNullOrWhiteSpace(request.Establecimiento) || string.IsNullOrWhiteSpace(request.PuntoEmision))
        {
            throw new InvalidOperationException("Debes seleccionar establecimiento y punto de emision para la liquidacion de compra.");
        }

        var empresa = await empresaRepository.GetCurrentAsync(cancellationToken)
            ?? throw new InvalidOperationException("No existe una empresa activa configurada para emitir la liquidacion.");

        var establecimiento = request.Establecimiento.Trim();
        var puntoEmision = request.PuntoEmision.Trim();
        var punto = empresa.PuntosEmision.FirstOrDefault(current =>
            string.Equals(current.Establecimiento, establecimiento, StringComparison.Ordinal) &&
            string.Equals(current.PuntoEmision, puntoEmision, StringComparison.Ordinal));

        if (punto is null)
        {
            throw new InvalidOperationException("El establecimiento y punto de emision seleccionados no pertenecen a la empresa activa.");
        }

        return (establecimiento, puntoEmision, "000000000");
    }

    private static (string Establecimiento, string PuntoEmision, string Secuencial) ParseNumeroComprobante(string? numeroComprobante)
    {
        var normalized = numeroComprobante?.Trim() ?? string.Empty;
        var parts = normalized.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 ||
            parts[0].Length != 3 ||
            parts[1].Length != 3 ||
            parts[2].Length != 9 ||
            parts.Any(part => !part.All(char.IsDigit)))
        {
            throw new InvalidOperationException("El numero de comprobante debe tener el formato 001-001-000000001.");
        }

        return (parts[0], parts[1], parts[2]);
    }

    private static decimal RoundSubtotal(IEnumerable<CompraDetalle> detalles, decimal ivaRate)
    {
        return Math.Round(
            detalles.Where(detalle => detalle.PorcentajeIva == ivaRate)
                .Sum(detalle => detalle.CostoTotalSinImpuesto),
            2,
            MidpointRounding.AwayFromZero);
    }

    private static CompraResponse MapToResponse(Compra compra)
    {
        return new CompraResponse
        {
            Id = compra.Id,
            ProveedorId = compra.ProveedorId,
            BodegaId = compra.BodegaId,
            NaturalezaCompra = compra.NaturalezaCompra.ToString(),
            FormaPagoCompra = compra.FormaPagoCompra.ToString(),
            RequiereBancarizacion = compra.RequiereBancarizacion,
            TipoDocumentoCodigo = compra.TipoDocumentoCodigo,
            TipoComprobanteSRI = compra.TipoComprobanteSRI,
            SustentoTributarioSRI = compra.SustentoTributarioSRI,
            TipoDocumentoNombre = CompraDocumentTypes.GetName(compra.TipoDocumentoCodigo),
            NumeroComprobante = compra.NumeroComprobante,
            ClaveAccesoProveedor = compra.ClaveAccesoProveedor,
            ClaveAccesoGenerada = compra.ClaveAccesoGenerada,
            NumeroAutorizacion = compra.NumeroAutorizacion,
            EstadoSri = compra.EstadoSri?.ToString(),
            MensajeEstado = compra.MensajeEstado,
            FormaPago = compra.FormaPagoSriCodigo,
            Observacion = compra.Observacion,
            FechaEmision = compra.FechaEmision,
            SubtotalIva0 = compra.SubtotalIva0,
            SubtotalIva5 = compra.SubtotalIva5,
            SubtotalIva8 = compra.SubtotalIva8,
            SubtotalIva15 = compra.SubtotalIva15,
            TotalDescuento = compra.TotalDescuento,
            TotalImpuestos = compra.TotalImpuestos,
            ImporteTotal = compra.ImporteTotal,
            EstadoCompra = compra.EstadoCompra.ToString(),
            CreatedAt = compra.CreatedAt,
            Detalles = compra.Detalles.Select(detalle => new CompraDetalleResponse
            {
                Id = detalle.Id,
                ProductoId = detalle.ProductoId,
                NaturalezaCompra = detalle.NaturalezaCompra.ToString(),
                NombreActivo = detalle.NombreActivo,
                CategoriaSriActivo = detalle.CategoriaSriActivo,
                SerieUbicacionActivo = detalle.SerieUbicacionActivo,
                ProductoCodigo = detalle.ProductoCodigo,
                ProductoNombre = detalle.ProductoNombre,
                CodigoIva = detalle.CodigoIva,
                PorcentajeIva = detalle.PorcentajeIva,
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario,
                Descuento = detalle.Descuento,
                CostoTotalSinImpuesto = detalle.CostoTotalSinImpuesto,
                TotalImpuesto = detalle.TotalImpuesto,
                TotalLinea = detalle.TotalLinea
            }).ToArray()
        };
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DomainNaturalezaCompra ToDomainNaturaleza(NaturalezaCompra naturalezaCompra)
    {
        return naturalezaCompra switch
        {
            NaturalezaCompra.ActivoFijo => DomainNaturalezaCompra.ActivoFijo,
            NaturalezaCompra.GastoServicio => DomainNaturalezaCompra.GastoServicio,
            _ => DomainNaturalezaCompra.MercaderiaInventario
        };
    }

    private static DomainFormaPagoCompra ToDomainFormaPago(FormaPagoCompra formaPagoCompra)
    {
        return formaPagoCompra switch
        {
            FormaPagoCompra.TransferenciaBancaria => DomainFormaPagoCompra.TransferenciaBancaria,
            FormaPagoCompra.Cheque => DomainFormaPagoCompra.Cheque,
            FormaPagoCompra.TarjetaCredito => DomainFormaPagoCompra.TarjetaCredito,
            FormaPagoCompra.CreditoProveedores => DomainFormaPagoCompra.CreditoProveedores,
            _ => DomainFormaPagoCompra.ContadoEfectivo
        };
    }

    private static string ResolveSriFormaPago(FormaPagoCompra formaPagoCompra, string? currentSriCode)
    {
        return formaPagoCompra switch
        {
            FormaPagoCompra.TransferenciaBancaria => SriCatalogCodes.FormaPagoTransferencia,
            FormaPagoCompra.Cheque => SriCatalogCodes.FormaPagoTransferencia,
            FormaPagoCompra.TarjetaCredito => SriCatalogCodes.FormaPagoTarjetaCredito,
            FormaPagoCompra.CreditoProveedores => SriCatalogCodes.FormaPagoTransferencia,
            _ => SriCatalogCodes.NormalizeFormaPagoCode(currentSriCode) ?? SriCatalogCodes.FormaPagoEfectivo
        };
    }

    public async Task<PagedResultResponse<CuentaPorPagarResponse>> GetCuentasPorPagarAsync(string? term, Guid? proveedorId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var page = await cuentaPorPagarRepository.GetPagedAsync(term, proveedorId, skip, take, cancellationToken);
        return new PagedResultResponse<CuentaPorPagarResponse>
        {
            Items = page.Items.Select(MapCuentaResponse).ToArray(),
            TotalCount = page.TotalCount,
            Skip = page.Skip,
            Take = page.Take
        };
    }

    public async Task<CuentasPorPagarResumenResponse> GetCuentasPorPagarResumenAsync(CancellationToken cancellationToken = default)
    {
        var resumen = await cuentaPorPagarRepository.GetResumenAsync(cancellationToken);
        return new CuentasPorPagarResumenResponse
        {
            TotalPorVencer = resumen.TotalPorVencer,
            TotalVencido = resumen.TotalVencido,
            TotalPendientes = resumen.TotalPendientes,
            TotalVencidas = resumen.TotalVencidas
        };
    }

    public async Task<CuentaPorPagarResponse> RegistrarAbonoAsync(RegistrarAbonoCxPRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CuentaPorPagarId == Guid.Empty)
        {
            throw new InvalidOperationException("La cuenta por pagar es obligatoria.");
        }

        var formaPago = SriCatalogCodes.NormalizeFormaPagoCode(request.FormaPago)
            ?? throw new InvalidOperationException("La forma de pago del abono no esta mapeada a un codigo SRI valido.");

        var pago = new PagoCxP(
            Guid.NewGuid(),
            request.CuentaPorPagarId,
            request.FechaPago,
            request.MontoPagado,
            formaPago,
            request.CuentaContableSalidaId,
            NormalizeOptional(request.NumeroComprobantePago),
            NormalizeOptional(request.ReferenciaTransaccion),
            DateTimeOffset.UtcNow,
            currentUserAccessor.GetRequiredUserId());

        var cuenta = await cuentaPorPagarRepository.RegistrarAbonoAsync(pago, currentUserAccessor.GetRequiredUserId(), cancellationToken);
        await RegistrarAsientoPagoCxPAsync(cuenta, pago, cancellationToken);
        return MapCuentaResponse(cuenta);
    }

    private async Task RegistrarAsientoPagoCxPAsync(CuentaPorPagar cuenta, PagoCxP pago, CancellationToken cancellationToken)
    {
        var cuentas = await contabilidadRepository.GetCuentasAceptablesAsync(cancellationToken);
        var cuentaProveedor = cuentas.FirstOrDefault(current => current.Codigo == "2.1.02.01" || current.Codigo == "2.1.01.01")
            ?? throw new InvalidOperationException("No existe la cuenta contable de Cuentas por Pagar Proveedores.");

        if (!pago.CuentaContableSalidaId.HasValue ||
            pago.CuentaContableSalidaId.Value == Guid.Empty ||
            cuentas.All(current => current.Id != pago.CuentaContableSalidaId.Value))
        {
            throw new InvalidOperationException("Selecciona una cuenta monetaria valida para registrar el pago.");
        }

        var documentoSoporte = BuildDocumentoSoporte(pago);

        await contabilidadRepository.CrearAsientoAsync(new CrearAsientoRequest
        {
            FechaContable = pago.FechaPago.Date,
            Concepto = $"Pago proveedor {cuenta.ProveedorNombre} comprobante {cuenta.NumeroComprobante}",
            ModuloOrigen = "Compras",
            DocumentoSoporte = documentoSoporte,
            Estado = "Posteado",
            Detalles =
            [
                new CrearAsientoDetalleRequest
                {
                    CuentaContableId = cuentaProveedor.Id,
                    Debe = pago.MontoPagado,
                    Haber = 0m
                },
                new CrearAsientoDetalleRequest
                {
                    CuentaContableId = pago.CuentaContableSalidaId.Value,
                    Debe = 0m,
                    Haber = pago.MontoPagado
                }
            ]
        }, cancellationToken);
    }

    private static string? BuildDocumentoSoporte(PagoCxP pago)
    {
        var soporte = NormalizeOptional(pago.NumeroComprobantePago) ?? NormalizeOptional(pago.ReferenciaTransaccion);
        return soporte is null || soporte.Length <= 49
            ? soporte
            : soporte[..49];
    }

    private static CuentaPorPagarResponse MapCuentaResponse(CuentaPorPagar cuenta)
    {
        return new CuentaPorPagarResponse
        {
            Id = cuenta.Id,
            CompraId = cuenta.CompraId,
            ProveedorId = cuenta.ProveedorId,
            ProveedorIdentificacion = cuenta.ProveedorIdentificacion,
            ProveedorNombre = cuenta.ProveedorNombre,
            FechaEmision = cuenta.FechaEmision,
            FechaVence = cuenta.FechaVence,
            MontoOriginal = cuenta.MontoOriginal,
            SaldoActual = cuenta.SaldoActual,
            EstadoDeuda = cuenta.EstadoDeuda.ToString(),
            NumeroComprobante = cuenta.NumeroComprobante,
            EstaVencida = cuenta.EstadoDeuda == EstadoDeuda.Vencida,
            Pagos = cuenta.Pagos.Select(pago => new PagoCxPResponse
            {
                Id = pago.Id,
                FechaPago = pago.FechaPago,
                MontoPagado = pago.MontoPagado,
                FormaPago = pago.FormaPago,
                CuentaContableSalidaId = pago.CuentaContableSalidaId,
                NumeroComprobantePago = pago.NumeroComprobantePago,
                ReferenciaTransaccion = pago.ReferenciaTransaccion
            }).ToArray()
        };
    }
}

