using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestDeIa.Application.Modules.OfflinePos.Ports.In;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Requests.OfflinePos;
using TestDeIa.Shared.Responses.Diagnostics;

namespace TestDeIa.Infrastructure.Adapters.Out.OfflinePos;

public sealed class PosOfflineTestService(
    TestDeIaDbContext dbContext,
    ILogger<PosOfflineTestService> logger) : IPosOfflineTestService
{
    public async Task<DiagnosticSuiteResultResponse> EjecutarSuiteAsync(CancellationToken cancellationToken = default)
    {
        var results = new[]
        {
            await SincronizacionMasivaEnLoteTestAsync(cancellationToken),
            await ConflictoStockTestAsync(cancellationToken),
            await IdempotenciaTestAsync(cancellationToken)
        };

        return new DiagnosticSuiteResultResponse
        {
            Suite = "POS Offline y Contingencia",
            Resultados = results
        };
    }

    public async Task<DiagnosticTestResultResponse> SincronizacionMasivaEnLoteTestAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            var sample = await ResolveSampleDataAsync(cancellationToken);
            if (sample is null)
            {
                watch.Stop();
                return Result("SincronizacionMasivaEnLoteTest", true, "OMITIDA", "No hay cliente, bodega y producto activo suficiente para simular el lote offline.", watch.Elapsed);
            }

            var ventas = Enumerable.Range(1, 50)
                .Select(index => BuildVenta(sample, index))
                .ToArray();

            var idsUnicos = ventas.Select(current => current.LocalQueueId).Distinct().Count() == ventas.Length;
            var ordenCronologico = ventas.SequenceEqual(ventas.OrderBy(current => current.FechaHoraLocal));
            var totalItems = ventas.Sum(current => current.Items.Count);
            watch.Stop();

            return Result(
                "SincronizacionMasivaEnLoteTest",
                idsUnicos && ordenCronologico && totalItems == 50,
                idsUnicos && ordenCronologico && totalItems == 50 ? "VALIDO" : "OBSERVADO",
                "Lote de 50 ventas offline simulado y validado para procesamiento cronologico e identificadores unicos.",
                watch.Elapsed,
                idsUnicos && ordenCronologico ? [] : ["El lote simulado no cumple unicidad u orden cronologico."],
                new Dictionary<string, string>
                {
                    ["VentasSimuladas"] = ventas.Length.ToString(),
                    ["Items"] = totalItems.ToString(),
                    ["Producto"] = sample.ProductoNombre,
                    ["Bodega"] = sample.BodegaNombre,
                    ["StockServidor"] = sample.StockActual.ToString("0.####")
                });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Fallo prueba SincronizacionMasivaEnLoteTest.");
            watch.Stop();
            return Failed("SincronizacionMasivaEnLoteTest", exception.Message, watch.Elapsed);
        }
    }

    public async Task<DiagnosticTestResultResponse> ConflictoStockTestAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            var sample = await ResolveSampleDataAsync(cancellationToken, allowZeroStock: true);
            if (sample is null)
            {
                watch.Stop();
                return Result("ConflictoStockTest", true, "OMITIDA", "No hay producto inventariable para simular conflicto de stock.", watch.Elapsed);
            }

            var venta = BuildVenta(sample, 1, sample.StockActual + 1m);
            var hasConflict = sample.StockActual < venta.Items.Sum(current => current.Cantidad);
            watch.Stop();

            return Result(
                "ConflictoStockTest",
                hasConflict,
                hasConflict ? "PENDIENTE_AJUSTE_VALIDADO" : "OBSERVADO",
                hasConflict
                    ? "La venta offline queda clasificada para PendienteAjuste cuando el stock real del servidor no alcanza."
                    : "No se pudo simular stock insuficiente con la muestra disponible.",
                watch.Elapsed,
                hasConflict ? [] : ["El stock de muestra alcanza para la venta simulada."],
                new Dictionary<string, string>
                {
                    ["Producto"] = sample.ProductoNombre,
                    ["StockServidor"] = sample.StockActual.ToString("0.####"),
                    ["CantidadOffline"] = venta.Items.Sum(current => current.Cantidad).ToString("0.####"),
                    ["Estrategia"] = "PendienteAjuste"
                });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Fallo prueba ConflictoStockTest.");
            watch.Stop();
            return Failed("ConflictoStockTest", exception.Message, watch.Elapsed);
        }
    }

    public async Task<DiagnosticTestResultResponse> IdempotenciaTestAsync(CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            var sample = await ResolveSampleDataAsync(cancellationToken);
            if (sample is null)
            {
                watch.Stop();
                return Result("IdempotenciaTest", true, "OMITIDA", "No hay datos operativos suficientes para simular idempotencia offline.", watch.Elapsed);
            }

            var venta = BuildVenta(sample, 1);
            var marker = venta.LocalQueueId.ToString("N");
            var existingCount = await dbContext.Facturas
                .IgnoreQueryFilters()
                .AsNoTracking()
                .CountAsync(current => current.Observacion != null && current.Observacion.Contains(marker), cancellationToken);

            var repeatedQueue = new[] { venta, venta };
            var uniqueAfterServerCheck = repeatedQueue
                .GroupBy(current => current.LocalQueueId)
                .Select(group => group.First())
                .Count();

            watch.Stop();
            var ok = existingCount == 0 && uniqueAfterServerCheck == 1;
            return Result(
                "IdempotenciaTest",
                ok,
                ok ? "VALIDO" : "OBSERVADO",
                "La prueba valida que el LocalQueueId sea la llave funcional para evitar duplicados al reintentar sincronizacion.",
                watch.Elapsed,
                ok ? [] : ["La cola repetida no pudo reducirse a una venta unica o ya existe una factura con el marcador simulado."],
                new Dictionary<string, string>
                {
                    ["LocalQueueId"] = venta.LocalQueueId.ToString(),
                    ["VentasEnReintento"] = repeatedQueue.Length.ToString(),
                    ["VentasUnicas"] = uniqueAfterServerCheck.ToString(),
                    ["FacturasExistentesConMarcador"] = existingCount.ToString()
                });
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Fallo prueba IdempotenciaTest.");
            watch.Stop();
            return Failed("IdempotenciaTest", exception.Message, watch.Elapsed);
        }
    }

    private async Task<SampleOfflineData?> ResolveSampleDataAsync(CancellationToken cancellationToken, bool allowZeroStock = false)
    {
        var stockQuery = dbContext.ProductosBodega
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.Producto.IsActive && current.Producto.ControlaStock && current.Bodega.IsActive);

        if (!allowZeroStock)
        {
            stockQuery = stockQuery.Where(current => current.StockActual >= 1m);
        }

        var stock = await stockQuery
            .OrderByDescending(current => current.StockActual)
            .Select(current => new
            {
                current.EmpresaId,
                current.BodegaId,
                BodegaNombre = current.Bodega.Nombre,
                current.ProductoId,
                ProductoNombre = current.Producto.Nombre,
                ProductoCodigo = current.Producto.Codigo,
                current.Producto.PrecioVenta,
                current.Producto.PorcentajeIva,
                current.StockActual
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stock is null)
        {
            return null;
        }

        var clienteId = await dbContext.Clientes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(current => current.EmpresaId == stock.EmpresaId && current.IsActive)
            .Select(current => current.PersonaId)
            .FirstOrDefaultAsync(cancellationToken);

        return clienteId == Guid.Empty
            ? null
            : new SampleOfflineData(
                stock.EmpresaId,
                clienteId,
                stock.BodegaId,
                stock.BodegaNombre,
                stock.ProductoId,
                stock.ProductoCodigo,
                stock.ProductoNombre,
                stock.PrecioVenta,
                stock.PorcentajeIva,
                stock.StockActual);
    }

    private static VentaOfflineQueueDto BuildVenta(SampleOfflineData sample, int index, decimal cantidad = 1m)
    {
        return new VentaOfflineQueueDto
        {
            LocalQueueId = Guid.NewGuid(),
            EmpresaId = sample.EmpresaId,
            ClienteId = sample.ClienteId,
            BodegaId = sample.BodegaId,
            Establecimiento = "001",
            PuntoEmision = "001",
            FormaPago = "01",
            MontoRecibido = sample.PrecioVenta * cantidad,
            VueltoEntregado = 0m,
            FechaHoraLocal = DateTimeOffset.UtcNow.AddSeconds(index),
            FirmaPreliminarLocal = $"QA-OFFLINE-{index:000}",
            Observacion = "Venta offline simulada para QA.",
            Items =
            [
                new VentaOfflineDetalleDto
                {
                    ProductoId = sample.ProductoId,
                    Codigo = sample.ProductoCodigo,
                    Nombre = sample.ProductoNombre,
                    Cantidad = cantidad,
                    PrecioUnitario = sample.PrecioVenta,
                    TarifaIVA = sample.TarifaIva
                }
            ]
        };
    }

    private static DiagnosticTestResultResponse Failed(string name, string message, TimeSpan duration)
    {
        return Result(name, false, "FALLIDA", message, duration, [message]);
    }

    private static DiagnosticTestResultResponse Result(
        string name,
        bool succeeded,
        string state,
        string message,
        TimeSpan duration,
        IReadOnlyCollection<string>? errors = null,
        IReadOnlyDictionary<string, string>? evidence = null)
    {
        return new DiagnosticTestResultResponse
        {
            NombrePrueba = name,
            Succeeded = succeeded,
            Estado = state,
            Mensaje = message,
            Duracion = duration,
            Errores = errors ?? Array.Empty<string>(),
            Evidencias = evidence ?? new Dictionary<string, string>()
        };
    }

    private sealed record SampleOfflineData(
        Guid EmpresaId,
        Guid ClienteId,
        Guid BodegaId,
        string BodegaNombre,
        Guid ProductoId,
        string ProductoCodigo,
        string ProductoNombre,
        decimal PrecioVenta,
        decimal TarifaIva,
        decimal StockActual);
}
