using System.Data;
using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Domain.Modules.Inventario;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Responses.Facturacion;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

public sealed class NotaCreditoService : INotaCreditoService
{
    private const string TipoDocumentoNotaCredito = "04";
    private const string TipoDocumentoFactura = "01";

    private readonly TestDeIaDbContext dbContext;
    private readonly NotaCreditoXmlGenerator xmlGenerator;

    public NotaCreditoService(TestDeIaDbContext dbContext, NotaCreditoXmlGenerator xmlGenerator)
    {
        this.dbContext = dbContext;
        this.xmlGenerator = xmlGenerator;
    }

    public async Task<NotaCreditoOrigenResponseDto> GetFacturaOrigenAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        if (facturaId == Guid.Empty)
        {
            throw new InvalidOperationException("La factura origen es obligatoria.");
        }

        var factura = await dbContext.Facturas
            .AsNoTracking()
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == facturaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la factura origen.");

        if (factura.Estado != FacturaEstado.AUTORIZADO)
        {
            throw new InvalidOperationException("Solo se puede emitir nota de credito sobre facturas autorizadas.");
        }

        var detalleIds = factura.Detalles.Select(current => current.Id).ToArray();
        var cantidadesDevueltasPrevias = await dbContext.ComprobanteDetalle
            .AsNoTracking()
            .Where(current =>
                current.FacturaDetalleOrigenId.HasValue &&
                detalleIds.Contains(current.FacturaDetalleOrigenId.Value) &&
                current.ComprobanteCabecera.ComprobanteModificadoId == factura.Id &&
                current.ComprobanteCabecera.Estado != FacturaEstado.RECHAZADO)
            .GroupBy(current => current.FacturaDetalleOrigenId!.Value)
            .Select(group => new
            {
                FacturaDetalleId = group.Key,
                Cantidad = group.Sum(current => current.Cantidad)
            })
            .ToDictionaryAsync(current => current.FacturaDetalleId, current => current.Cantidad, cancellationToken);

        return new NotaCreditoOrigenResponseDto
        {
            FacturaId = factura.Id,
            NumeroComprobante = $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
            ClienteNombre = factura.ClienteNombre,
            FechaEmision = factura.FechaEmision,
            Total = factura.Total,
            Detalles = factura.Detalles
                .OrderBy(current => current.NombreProducto)
                .Select(current =>
                {
                    cantidadesDevueltasPrevias.TryGetValue(current.Id, out var devuelta);
                    var disponible = Math.Max(0m, current.Cantidad - devuelta);
                    return new NotaCreditoOrigenDetalleResponseDto
                    {
                        FacturaDetalleId = current.Id,
                        CodigoProducto = current.CodigoProducto,
                        NombreProducto = current.NombreProducto,
                        CantidadFacturada = current.Cantidad,
                        CantidadDevuelta = devuelta,
                        CantidadDisponible = disponible,
                        PrecioUnitario = current.PrecioUnitario,
                        Subtotal = current.Subtotal,
                        IvaValor = current.IvaValor,
                        Total = current.Total
                    };
                })
                .ToArray()
        };
    }

    public async Task<NotaCreditoResponseDto> CrearNotaCredito(NotaCreditoRequestDto dto, CancellationToken cancellationToken = default)
    {
        ValidateRequest(dto);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var factura = await dbContext.Facturas
            .Include(current => current.Detalles)
            .FirstOrDefaultAsync(current => current.Id == dto.FacturaId, cancellationToken)
            ?? throw new InvalidOperationException("No se encontro la factura origen para generar la nota de credito.");

        if (factura.Estado != FacturaEstado.AUTORIZADO)
        {
            throw new InvalidOperationException("Solo se puede emitir nota de credito sobre facturas autorizadas.");
        }

        var detalleRequests = dto.Detalles
            .GroupBy(current => current.FacturaDetalleId)
            .ToDictionary(group => group.Key, group => group.Sum(current => current.Cantidad));

        var detallesOrigen = factura.Detalles
            .Where(current => detalleRequests.ContainsKey(current.Id))
            .ToArray();

        if (detallesOrigen.Length != detalleRequests.Count)
        {
            throw new InvalidOperationException("La nota de credito contiene detalles que no pertenecen a la factura origen.");
        }

        var detalleIds = detalleRequests.Keys.ToArray();
        var cantidadesDevueltasPrevias = await dbContext.ComprobanteDetalle
            .AsNoTracking()
            .Where(current =>
                current.FacturaDetalleOrigenId.HasValue &&
                detalleIds.Contains(current.FacturaDetalleOrigenId.Value) &&
                current.ComprobanteCabecera.ComprobanteModificadoId == factura.Id &&
                current.ComprobanteCabecera.Estado != FacturaEstado.RECHAZADO)
            .GroupBy(current => current.FacturaDetalleOrigenId!.Value)
            .Select(group => new
            {
                FacturaDetalleId = group.Key,
                Cantidad = group.Sum(current => current.Cantidad)
            })
            .ToDictionaryAsync(current => current.FacturaDetalleId, current => current.Cantidad, cancellationToken);

        var productoIds = detallesOrigen.Select(current => current.ProductoId).Distinct().ToArray();
        var productos = await dbContext.Productos
            .Include(current => current.ProductosBodega)
            .Where(current => productoIds.Contains(current.Id))
            .ToDictionaryAsync(current => current.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var secuencial = await ReserveNextSecuencialAsync(
            factura.EmpresaId,
            factura.Establecimiento,
            factura.PuntoEmision,
            now,
            cancellationToken);

        var notaCredito = BuildCabecera(factura, dto.MotivoModificacion.Trim(), secuencial, now);

        foreach (var detalleOrigen in detallesOrigen)
        {
            var cantidadDevuelta = detalleRequests[detalleOrigen.Id];
            cantidadesDevueltasPrevias.TryGetValue(detalleOrigen.Id, out var cantidadDevueltaPrevia);
            if (cantidadDevuelta <= 0 || cantidadDevuelta + cantidadDevueltaPrevia > detalleOrigen.Cantidad)
            {
                throw new InvalidOperationException($"La cantidad a devolver de {detalleOrigen.NombreProducto} no es valida.");
            }

            var ratio = cantidadDevuelta / detalleOrigen.Cantidad;
            var subtotal = Math.Round(detalleOrigen.Subtotal * ratio, 2, MidpointRounding.AwayFromZero);
            var descuento = Math.Round(detalleOrigen.Descuento * ratio, 2, MidpointRounding.AwayFromZero);
            var iva = Math.Round(detalleOrigen.IvaValor * ratio, 2, MidpointRounding.AwayFromZero);
            var total = subtotal + iva;

            notaCredito.Detalles.Add(new ComprobanteDetalleEntity
            {
                Id = Guid.NewGuid(),
                ComprobanteCabeceraId = notaCredito.Id,
                FacturaDetalleOrigenId = detalleOrigen.Id,
                ProductoId = detalleOrigen.ProductoId,
                CodigoProducto = detalleOrigen.CodigoProducto,
                NombreProducto = detalleOrigen.NombreProducto,
                CodigoIva = detalleOrigen.CodigoIva,
                PorcentajeIva = detalleOrigen.PorcentajeIva,
                Cantidad = cantidadDevuelta,
                PrecioUnitario = detalleOrigen.PrecioUnitario,
                Descuento = descuento,
                Subtotal = subtotal,
                IvaValor = iva,
                Total = total
            });

            notaCredito.Subtotal += subtotal;
            notaCredito.TotalDescuento += descuento;
            notaCredito.IvaTotal += iva;
            notaCredito.Total += total;

            if (productos.TryGetValue(detalleOrigen.ProductoId, out var producto) && producto.ControlaStock)
            {
                IncrementarExistenciaYRegistrarKardex(
                    factura,
                    producto,
                    cantidadDevuelta,
                    $"Nota credito {factura.Establecimiento}-{factura.PuntoEmision}-{secuencial:000000000}",
                    now);
            }
        }

        notaCredito.ClaveAcceso = xmlGenerator.GenerarClaveAcceso(notaCredito);
        notaCredito.XmlGenerado = xmlGenerator.BuildUnsignedXml(notaCredito);

        dbContext.ComprobanteCabecera.Add(notaCredito);
        dbContext.ColaProcesamientoSRI.Add(new ColaProcesamientoSriEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = factura.EmpresaId,
            ComprobanteId = notaCredito.Id,
            TipoDocumentoId = TipoDocumentoNotaCredito,
            Estado = "Pendiente",
            CreatedAt = now,
            Mensaje = "Nota de credito generada y pendiente de procesamiento SRI."
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new NotaCreditoResponseDto
        {
            ComprobanteId = notaCredito.Id,
            FacturaOrigenId = factura.Id,
            TipoDocumentoId = TipoDocumentoNotaCredito,
            NumeroComprobante = $"{notaCredito.Establecimiento}-{notaCredito.PuntoEmision}-{notaCredito.Secuencial:000000000}",
            ClaveAcceso = notaCredito.ClaveAcceso,
            Estado = notaCredito.Estado.ToApiValue(),
            Total = notaCredito.Total
        };
    }

    private static void ValidateRequest(NotaCreditoRequestDto dto)
    {
        if (dto.FacturaId == Guid.Empty)
        {
            throw new InvalidOperationException("La factura origen es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(dto.MotivoModificacion))
        {
            throw new InvalidOperationException("El motivo de modificacion es obligatorio.");
        }

        if (dto.Detalles.Count == 0)
        {
            throw new InvalidOperationException("La nota de credito debe contener al menos un item devuelto.");
        }

        if (dto.Detalles.Any(current => current.FacturaDetalleId == Guid.Empty || current.Cantidad <= 0))
        {
            throw new InvalidOperationException("La nota de credito contiene items invalidos.");
        }
    }

    private static ComprobanteCabeceraEntity BuildCabecera(
        FacturaEntity factura,
        string motivo,
        long secuencial,
        DateTimeOffset now)
    {
        return new ComprobanteCabeceraEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = factura.EmpresaId,
            TipoDocumentoId = TipoDocumentoNotaCredito,
            Secuencial = secuencial,
            Establecimiento = factura.Establecimiento,
            PuntoEmision = factura.PuntoEmision,
            ComprobanteModificadoId = factura.Id,
            MotivoModificacion = motivo,
            CodDocModificado = TipoDocumentoFactura,
            NumDocModificado = $"{factura.Establecimiento}-{factura.PuntoEmision}-{factura.Secuencial:000000000}",
            FechaEmisionDocSustento = factura.FechaEmision,
            RucEmisor = factura.RucEmisor,
            RazonSocialEmisor = factura.RazonSocialEmisor,
            NombreComercialEmisor = factura.NombreComercialEmisor,
            DireccionMatrizEmisor = factura.DireccionMatrizEmisor,
            DireccionEstablecimientoEmisor = factura.DireccionEstablecimientoEmisor,
            AmbienteSri = factura.AmbienteSri,
            TipoEmision = factura.TipoEmision,
            ObligadoContabilidad = factura.ObligadoContabilidad,
            ClienteTipoIdentificacion = factura.ClienteTipoIdentificacion,
            ClienteIdentificacion = factura.ClienteIdentificacion,
            ClienteNombre = factura.ClienteNombre,
            ClienteDireccion = factura.ClienteDireccion,
            Estado = FacturaEstado.NO_FIRMADO,
            FechaEmision = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private void IncrementarExistenciaYRegistrarKardex(
        FacturaEntity factura,
        ProductoEntity producto,
        decimal cantidad,
        string referencia,
        DateTimeOffset now)
    {
        var existencia = producto.ProductosBodega.FirstOrDefault(current => current.BodegaId == factura.BodegaId);
        if (existencia is null)
        {
            existencia = new ProductoBodegaEntity
            {
                ProductoId = producto.Id,
                BodegaId = factura.BodegaId,
                EmpresaId = factura.EmpresaId,
                StockActual = 0
            };
            producto.ProductosBodega.Add(existencia);
            dbContext.ProductosBodega.Add(existencia);
        }

        var stockAnterior = existencia.StockActual;
        existencia.StockActual += cantidad;
        producto.UpdatedAt = now;

        dbContext.KardexMovimientos.Add(new KardexMovimientoEntity
        {
            Id = Guid.NewGuid(),
            EmpresaId = factura.EmpresaId,
            ProductoId = producto.Id,
            BodegaId = factura.BodegaId,
            TipoMovimiento = TipoMovimientoInventario.DevolucionVenta,
            Concepto = TipoMovimientoInventario.DevolucionVenta,
            Referencia = referencia,
            CantidadEntrada = cantidad,
            CantidadSalida = 0m,
            SaldoCantidad = existencia.StockActual,
            CostoUnitario = producto.CostoPromedio,
            CostoTotal = Math.Round(cantidad * producto.CostoPromedio, 4, MidpointRounding.AwayFromZero),
            CostoPromedio = producto.CostoPromedio,
            StockAnterior = stockAnterior,
            StockNuevo = existencia.StockActual,
            SaldoValor = existencia.StockActual * producto.CostoPromedio,
            FacturaId = factura.Id,
            FechaMovimiento = now
        });
    }

    private async Task<long> ReserveNextSecuencialAsync(
        Guid empresaId,
        string establecimiento,
        string puntoEmision,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var secuencial = await dbContext.FacturaSecuenciales
            .FirstOrDefaultAsync(current =>
                current.EmpresaId == empresaId &&
                current.CodigoDocumento == TipoDocumentoNotaCredito &&
                current.Establecimiento == establecimiento &&
                current.PuntoEmision == puntoEmision,
                cancellationToken);

        if (secuencial is null)
        {
            secuencial = new FacturaSecuencialEntity
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                CodigoDocumento = TipoDocumentoNotaCredito,
                Establecimiento = establecimiento,
                PuntoEmision = puntoEmision,
                UltimoSecuencial = 0,
                CreatedAt = now
            };
            dbContext.FacturaSecuenciales.Add(secuencial);
        }

        secuencial.UltimoSecuencial++;
        secuencial.UpdatedAt = now;
        return secuencial.UltimoSecuencial;
    }
}
