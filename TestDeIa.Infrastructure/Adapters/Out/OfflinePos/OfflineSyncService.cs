using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Common;
using TestDeIa.Application.Modules.Facturacion.Ports.In;
using TestDeIa.Application.Modules.OfflinePos.Ports.In;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Infrastructure.Persistence.Entities;
using TestDeIa.Shared.Requests.Facturacion;
using TestDeIa.Shared.Requests.OfflinePos;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Responses.OfflinePos;

namespace TestDeIa.Infrastructure.Adapters.Out.OfflinePos;

public sealed class OfflineSyncService(
    TestDeIaDbContext dbContext,
    IFacturacionUseCase facturacionUseCase,
    ITenantContextAccessor tenantContextAccessor,
    ILogger<OfflineSyncService> logger) : IOfflineSyncService
{
    /// <summary>
    /// Sincroniza únicamente ventas pertenecientes a la empresa autenticada. La cola offline
    /// es entrada no confiable: su EmpresaId se contrasta con el tenant del JWT antes de consultar
    /// facturas o existencias y no se usa IgnoreQueryFilters fuera de consultas con tenant explícito.
    /// </summary>
    public async Task<OfflineSyncResultResponse> SincronizarVentasOfflineAsync(
        IReadOnlyCollection<VentaOfflineQueueDto> ventas,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var ordered = ventas.OrderBy(current => current.FechaHoraLocal).ToArray();
        var results = new List<OfflineSyncItemResultResponse>();
        var empresaActivaId = tenantContextAccessor.EmpresaId;

        logger.LogInformation(
            "Inicio sincronizacion POS offline. Cajeros={Cajeros}; Ventas={TotalVentas}; Inicio={InicioUtc}.",
            string.Join(",", ordered.SelectMany(current => current.Items.Select(item => item.UsuarioIdOperador)).Where(current => current.HasValue).Select(current => current!.Value).Distinct()),
            ordered.Length,
            startedAt);

        foreach (var venta in ordered)
        {
            try
            {
                if (!empresaActivaId.HasValue || empresaActivaId.Value == Guid.Empty || venta.EmpresaId != empresaActivaId.Value)
                {
                    results.Add(Failed(venta, "ErrorValidacion", "La venta offline no pertenece a la empresa activa."));
                    continue;
                }

                var existing = await FindExistingOfflineInvoiceAsync(venta.EmpresaId, venta, cancellationToken);
                if (existing is not null)
                {
                    results.Add(new OfflineSyncItemResultResponse
                    {
                        LocalQueueId = venta.LocalQueueId,
                        Succeeded = true,
                        FacturaId = existing.Id,
                        NumeroComprobante = $"{existing.Establecimiento}-{existing.PuntoEmision}-{existing.Secuencial:000000000}",
                        Estado = "DuplicadoOmitido",
                        Mensaje = "Venta offline ya sincronizada previamente; no se duplico la factura."
                    });

                    logger.LogInformation(
                        "Venta offline {LocalQueueId} omitida por idempotencia. Factura existente {FacturaId}.",
                        venta.LocalQueueId,
                        existing.Id);

                    continue;
                }

                var stockConflict = await FindStockConflictAsync(venta.EmpresaId, venta, cancellationToken);
                if (!string.IsNullOrWhiteSpace(stockConflict))
                {
                    results.Add(Failed(venta, "PendienteAjuste", stockConflict));
                    logger.LogWarning(
                        "Venta offline {LocalQueueId} marcada PendienteAjuste por conflicto de stock: {Conflicto}.",
                        venta.LocalQueueId,
                        stockConflict);

                    continue;
                }

                var request = new EmitirFacturaRequest
                {
                    ClienteId = venta.ClienteId,
                    BodegaId = venta.BodegaId,
                    Establecimiento = venta.Establecimiento,
                    PuntoEmision = venta.PuntoEmision,
                    FormaPago = venta.FormaPago,
                    MontoRecibido = venta.MontoRecibido,
                    VueltoEntregado = venta.VueltoEntregado,
                    Observacion = BuildOfflineObservation(venta),
                    Items = venta.Items.Select(current => new EmitirFacturaDetalleRequest
                    {
                        ProductoId = current.ProductoId,
                        Cantidad = current.Cantidad,
                        Descuento = current.Descuento,
                        PrecioUnitarioOverride = current.PrecioUnitario,
                        UsuarioIdOperador = current.UsuarioIdOperador
                    }).ToArray()
                };

                var response = await facturacionUseCase.EmitirFacturaAsync(request, cancellationToken);
                results.Add(new OfflineSyncItemResultResponse
                {
                    LocalQueueId = venta.LocalQueueId,
                    Succeeded = true,
                    FacturaId = response.FacturaId,
                    NumeroComprobante = response.NumeroComprobante,
                    Estado = response.Estado,
                    Mensaje = response.Mensaje
                });

                logger.LogInformation(
                    "Venta offline {LocalQueueId} sincronizada como factura {FacturaId}.",
                    venta.LocalQueueId,
                    response.FacturaId);
            }
            catch (StockInsuficienteException exception)
            {
                results.Add(Failed(venta, "ConflictoStock", exception.Message));
                logger.LogWarning(exception, "Venta offline {LocalQueueId} con conflicto de stock.", venta.LocalQueueId);
            }
            catch (InvalidOperationException exception)
            {
                results.Add(Failed(venta, "ErrorValidacion", exception.Message));
                logger.LogWarning(exception, "Venta offline {LocalQueueId} con error de validacion.", venta.LocalQueueId);
            }
            catch (Exception exception)
            {
                results.Add(Failed(venta, "Error", exception.Message));
                logger.LogError(exception, "Venta offline {LocalQueueId} no pudo sincronizarse.", venta.LocalQueueId);
            }
        }

        watch.Stop();
        logger.LogInformation(
            "Fin sincronizacion POS offline. Ventas={TotalVentas}; Procesadas={Procesadas}; Conflictos={Conflictos}; Errores={Errores}; DuracionMs={DuracionMs}.",
            ordered.Length,
            results.Count(current => current.Succeeded),
            results.Count(current => current.Estado is "ConflictoStock" or "PendienteAjuste"),
            results.Count(current => !current.Succeeded && current.Estado is not "ConflictoStock" and not "PendienteAjuste"),
            watch.ElapsedMilliseconds);

        return new OfflineSyncResultResponse
        {
            Recibidas = ordered.Length,
            Procesadas = results.Count(current => current.Succeeded),
            ConflictosStock = results.Count(current => current.Estado is "ConflictoStock" or "PendienteAjuste"),
            ErroresValidacion = results.Count(current => !current.Succeeded && current.Estado is not "ConflictoStock" and not "PendienteAjuste"),
            Resultados = results
        };
    }

    public async Task<PosOfflineCatalogoCacheResponse> ObtenerCatalogoPosOfflineAsync(
        Guid? bodegaId = null,
        CancellationToken cancellationToken = default)
    {
        var productosQuery = dbContext.Productos
            .AsNoTracking()
            .Include(current => current.ProductosBodega)
            .Where(current => current.IsActive);

        if (bodegaId.HasValue && bodegaId.Value != Guid.Empty)
        {
            productosQuery = productosQuery.Where(current =>
                !current.ControlaStock ||
                current.ProductosBodega.Any(stock => stock.BodegaId == bodegaId.Value));
        }

        var productos = await productosQuery
            .OrderBy(current => current.Nombre)
            .Take(1000)
            .Select(current => new StockLocalCacheDto
            {
                EmpresaId = current.EmpresaId,
                BodegaId = bodegaId ?? current.ProductosBodega.OrderByDescending(stock => stock.StockActual).Select(stock => stock.BodegaId).FirstOrDefault(),
                ProductoId = current.Id,
                CodigoBarra = current.Codigo,
                Nombre = current.Nombre,
                Precio = current.PrecioVenta,
                TarifaIVA = current.PorcentajeIva,
                StockDisponible = bodegaId.HasValue
                    ? current.ProductosBodega.Where(stock => stock.BodegaId == bodegaId.Value).Select(stock => stock.StockActual).FirstOrDefault()
                    : current.ProductosBodega.Sum(stock => stock.StockActual),
                ControlaStock = current.ControlaStock,
                CachedAt = DateTimeOffset.UtcNow
            })
            .ToArrayAsync(cancellationToken);

        var clientes = await dbContext.Clientes
            .AsNoTracking()
            .Include(current => current.Persona)
            .Where(current => current.IsActive)
            .OrderBy(current => current.Persona.RazonSocialONombresCompletos)
            .Take(500)
            .Select(current => new PosClienteResponse
            {
                ClienteId = current.PersonaId,
                PersonaId = current.PersonaId,
                TipoIdentificacion = current.Persona.TipoIdentificacion,
                Identificacion = current.Persona.Identificacion,
                NombreCompleto = current.Persona.RazonSocialONombresCompletos,
                NombreComercial = current.Persona.NombreComercial,
                Email = current.Persona.CorreoElectronicoPrincipal,
                Telefono = current.Persona.TelefonoCelular,
                Direccion = current.Persona.DireccionPrincipal,
                HasClienteExtension = true
            })
            .ToArrayAsync(cancellationToken);

        return new PosOfflineCatalogoCacheResponse
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            Productos = productos,
            Clientes = clientes
        };
    }

    private static OfflineSyncItemResultResponse Failed(VentaOfflineQueueDto venta, string estado, string message)
    {
        return new OfflineSyncItemResultResponse
        {
            LocalQueueId = venta.LocalQueueId,
            Succeeded = false,
            Estado = estado,
            Mensaje = message
        };
    }

    private static string BuildOfflineObservation(VentaOfflineQueueDto venta)
    {
        var marker = $"Venta offline {venta.LocalQueueId:N} registrada localmente {venta.FechaHoraLocal:yyyy-MM-dd HH:mm:ss}. Firma local: {venta.FirmaPreliminarLocal}";
        return string.IsNullOrWhiteSpace(venta.Observacion)
            ? marker
            : $"{venta.Observacion} | {marker}";
    }

    /// <summary>
    /// Desactiva el filtro global solo para la comprobación de idempotencia de la cola offline,
    /// manteniendo obligatorio el predicado por EmpresaId y el marcador LocalQueueId.
    /// </summary>
    private async Task<FacturaEntity?> FindExistingOfflineInvoiceAsync(Guid empresaId, VentaOfflineQueueDto venta, CancellationToken cancellationToken)
    {
        var marker = venta.LocalQueueId.ToString("N");
        return await dbContext.Facturas
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId && current.Observacion != null && current.Observacion.Contains(marker))
            .OrderByDescending(current => current.FechaEmision)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Consulta la existencia operativa de la empresa activa. Se desactiva el filtro global
    /// únicamente porque esta sincronización de sistema usa un DbContext de worker, y se restringe
    /// explícitamente por EmpresaId, BodegaId y los productos de la venta.
    /// </summary>
    private async Task<string?> FindStockConflictAsync(Guid empresaId, VentaOfflineQueueDto venta, CancellationToken cancellationToken)
    {
        var productIds = venta.Items.Select(current => current.ProductoId).Distinct().ToArray();
        var stockByProduct = await dbContext.ProductosBodega
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.EmpresaId == empresaId && current.BodegaId == venta.BodegaId && productIds.Contains(current.ProductoId))
            .Select(current => new
            {
                current.ProductoId,
                current.StockActual,
                current.Producto.Nombre,
                current.Producto.ControlaStock
            })
            .ToDictionaryAsync(current => current.ProductoId, cancellationToken);

        foreach (var item in venta.Items)
        {
            if (!stockByProduct.TryGetValue(item.ProductoId, out var stock))
            {
                continue;
            }

            if (stock.ControlaStock && stock.StockActual < item.Cantidad)
            {
                return $"Stock insuficiente para {stock.Nombre}. Disponible servidor: {stock.StockActual:0.####}; solicitado offline: {item.Cantidad:0.####}.";
            }
        }

        return null;
    }
}
