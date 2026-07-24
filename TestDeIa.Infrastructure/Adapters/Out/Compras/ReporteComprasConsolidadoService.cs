using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Compras.Ports.In;
using TestDeIa.Domain.Modules.Compras.Enums;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Compras;
using SharedNaturalezaCompra = TestDeIa.Shared.Compras.NaturalezaCompra;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class ReporteComprasConsolidadoService(TestDeIaDbContext dbContext) : IReporteComprasConsolidadoService
{
    public async Task<ReporteComprasConsolidadoResponse> ConsultarAsync(
        DateTime fechaInicio,
        DateTime fechaFin,
        SharedNaturalezaCompra? naturalezaCompra,
        CancellationToken cancellationToken = default)
    {
        if (fechaFin.Date < fechaInicio.Date)
        {
            throw new InvalidOperationException("La fecha final no puede ser menor a la fecha inicial.");
        }

        var desde = new DateTimeOffset(fechaInicio.Date, TimeSpan.Zero);
        var hasta = new DateTimeOffset(fechaFin.Date.AddDays(1), TimeSpan.Zero);
        var naturalezaDominio = naturalezaCompra.HasValue ? ToDomainNaturaleza(naturalezaCompra.Value) : (NaturalezaCompra?)null;

        var compras = await dbContext.Compras
            .AsNoTracking()
            .Include(compra => compra.Proveedor)
                .ThenInclude(proveedor => proveedor.Persona)
            .Include(compra => compra.Detalles)
            .Where(compra => compra.FechaEmision >= desde && compra.FechaEmision < hasta)
            .Where(compra => !naturalezaDominio.HasValue || compra.NaturalezaCompra == naturalezaDominio.Value)
            .OrderBy(compra => compra.FechaEmision)
            .ToArrayAsync(cancellationToken);

        var compraIds = compras.Select(compra => compra.Id).ToArray();

        var activos = await dbContext.ActivosFijos
            .AsNoTracking()
            .Include(activo => activo.CompraDetalle)
                .ThenInclude(detalle => detalle!.Compra)
                    .ThenInclude(compra => compra.Proveedor)
                        .ThenInclude(proveedor => proveedor.Persona)
            .Where(activo => activo.FechaAdquisicion >= fechaInicio.Date && activo.FechaAdquisicion < fechaFin.Date.AddDays(1))
            .Where(activo => !naturalezaDominio.HasValue || naturalezaDominio.Value == NaturalezaCompra.ActivoFijo)
            .OrderBy(activo => activo.FechaAdquisicion)
            .ToArrayAsync(cancellationToken);

        var cuentasPorPagar = await dbContext.CuentasPorPagar
            .AsNoTracking()
            .Include(cuenta => cuenta.Proveedor)
                .ThenInclude(proveedor => proveedor.Persona)
            .Include(cuenta => cuenta.Compra)
            .Include(cuenta => cuenta.Pagos)
            .Where(cuenta => cuenta.CompraId.HasValue && compraIds.Contains(cuenta.CompraId.Value))
            .ToArrayAsync(cancellationToken);

        var movimientosFisicos = BuildMovimientosFisicos(compras, activos);
        var flujos = BuildFlujosMonetarios(compras, cuentasPorPagar, desde, hasta);

        var baseIva0 = compras.Sum(compra => compra.SubtotalIva0);
        var baseIva15 = compras.Sum(compra => compra.SubtotalIva15);
        var montoIva = compras.Sum(compra => compra.TotalImpuestos);
        var totalCompras = compras.Sum(compra => compra.ImporteTotal);
        var totalAbonos = cuentasPorPagar.SelectMany(cuenta => cuenta.Pagos)
            .Where(pago => pago.FechaPago >= desde && pago.FechaPago < hasta)
            .Sum(pago => pago.MontoPagado);
        var contado = compras
            .Where(compra => compra.FormaPagoCompra != FormaPagoCompra.CreditoProveedores)
            .Sum(compra => compra.ImporteTotal);
        var pasivoAcumulado = cuentasPorPagar.Sum(cuenta => cuenta.SaldoActual);

        return new ReporteComprasConsolidadoResponse
        {
            FechaInicio = fechaInicio.Date,
            FechaFin = fechaFin.Date,
            NaturalezaCompra = naturalezaCompra?.ToString() ?? "Todos",
            DimensionFisica = new ReporteComprasDimensionFisicaResponse
            {
                TotalUnidadesInventario = movimientosFisicos
                    .Where(movimiento => movimiento.NaturalezaCompra == NaturalezaCompra.MercaderiaInventario.ToString())
                    .Sum(movimiento => movimiento.Cantidad),
                TotalImporteInventario = movimientosFisicos
                    .Where(movimiento => movimiento.NaturalezaCompra == NaturalezaCompra.MercaderiaInventario.ToString())
                    .Sum(movimiento => movimiento.Importe),
                TotalActivosDadosAlta = activos.Length,
                TotalImporteActivos = activos.Sum(activo => activo.CostoInicial),
                Movimientos = movimientosFisicos
            },
            DimensionMonetaria = new ReporteComprasDimensionMonetariaResponse
            {
                BaseIva0 = baseIva0,
                BaseIva15 = baseIva15,
                MontoIva = montoIva,
                TotalCompras = totalCompras,
                SalidaEfectivaCajaBanco = contado + totalAbonos,
                PasivoAcumuladoCxP = pasivoAcumulado,
                TotalAbonosCxP = totalAbonos,
                Flujos = flujos
            }
        };
    }

    private static IReadOnlyCollection<ReporteComprasMovimientoFisicoResponse> BuildMovimientosFisicos(
        IReadOnlyCollection<Persistence.Entities.CompraEntity> compras,
        IReadOnlyCollection<Persistence.Entities.ActivoFijoEntity> activos)
    {
        var inventario = compras
            .SelectMany(compra => compra.Detalles
                .Where(detalle => detalle.NaturalezaCompra == NaturalezaCompra.MercaderiaInventario)
                .Select(detalle => new ReporteComprasMovimientoFisicoResponse
                {
                    Fecha = compra.FechaEmision,
                    Tipo = "Kardex / Mercaderia",
                    NaturalezaCompra = detalle.NaturalezaCompra.ToString(),
                    Codigo = detalle.ProductoCodigo,
                    Descripcion = detalle.ProductoNombre,
                    Proveedor = BuildProveedor(compra.Proveedor),
                    ComprobanteSri = BuildComprobante(compra),
                    ClaveAccesoSri = compra.ClaveAccesoProveedor ?? compra.ClaveAccesoGenerada,
                    Cantidad = detalle.Cantidad,
                    Importe = detalle.CostoTotalSinImpuesto
                }));

        var activosFijos = activos.Select(activo =>
        {
            var compra = activo.CompraDetalle?.Compra;
            return new ReporteComprasMovimientoFisicoResponse
            {
                Fecha = new DateTimeOffset(activo.FechaAdquisicion, TimeSpan.Zero),
                Tipo = "Activo fijo",
                NaturalezaCompra = NaturalezaCompra.ActivoFijo.ToString(),
                Codigo = activo.CodigoActivo,
                Descripcion = activo.Nombre,
                Proveedor = compra is null ? "Sin proveedor" : BuildProveedor(compra.Proveedor),
                ComprobanteSri = compra is null ? "Sin comprobante" : BuildComprobante(compra),
                ClaveAccesoSri = compra?.ClaveAccesoProveedor ?? compra?.ClaveAccesoGenerada,
                Cantidad = 1m,
                Importe = activo.CostoInicial
            };
        });

        return inventario.Concat(activosFijos)
            .OrderBy(movimiento => movimiento.Fecha)
            .ThenBy(movimiento => movimiento.Tipo)
            .ToArray();
    }

    private static IReadOnlyCollection<ReporteComprasFlujoMonetarioResponse> BuildFlujosMonetarios(
        IReadOnlyCollection<Persistence.Entities.CompraEntity> compras,
        IReadOnlyCollection<Persistence.Entities.CuentaPorPagarEntity> cuentasPorPagar,
        DateTimeOffset desde,
        DateTimeOffset hasta)
    {
        var cuentasPorCompra = cuentasPorPagar
            .Where(cuenta => cuenta.CompraId.HasValue)
            .ToDictionary(cuenta => cuenta.CompraId!.Value);

        var comprasFlujo = compras.Select(compra =>
        {
            cuentasPorCompra.TryGetValue(compra.Id, out var cuenta);
            return new ReporteComprasFlujoMonetarioResponse
            {
                Fecha = compra.FechaEmision,
                Tipo = compra.FormaPagoCompra == FormaPagoCompra.CreditoProveedores ? "Compra a credito" : "Compra contado/bancarizada",
                Proveedor = BuildProveedor(compra.Proveedor),
                Documento = BuildComprobante(compra),
                FormaPago = compra.FormaPagoCompra.ToString(),
                TotalCompra = compra.ImporteTotal,
                Desembolso = compra.FormaPagoCompra == FormaPagoCompra.CreditoProveedores ? 0m : compra.ImporteTotal,
                SaldoPendiente = cuenta?.SaldoActual ?? 0m
            };
        });

        var pagos = cuentasPorPagar.SelectMany(cuenta => cuenta.Pagos
            .Where(pago => pago.FechaPago >= desde && pago.FechaPago < hasta)
            .Select(pago => new ReporteComprasFlujoMonetarioResponse
            {
                Fecha = pago.FechaPago,
                Tipo = "Abono CxP",
                Proveedor = BuildProveedor(cuenta.Proveedor),
                Documento = cuenta.Compra is null ? cuenta.Id.ToString("N")[..10] : BuildComprobante(cuenta.Compra),
                FormaPago = pago.FormaPago,
                TotalCompra = cuenta.MontoOriginal,
                Desembolso = pago.MontoPagado,
                SaldoPendiente = cuenta.SaldoActual,
                ComprobantePago = pago.NumeroComprobantePago ?? pago.ReferenciaTransaccion
            }));

        return comprasFlujo.Concat(pagos)
            .OrderBy(flujo => flujo.Fecha)
            .ThenBy(flujo => flujo.Tipo)
            .ToArray();
    }

    private static string BuildComprobante(Persistence.Entities.CompraEntity compra)
    {
        return $"{compra.Establecimiento}-{compra.PuntoEmision}-{compra.Secuencial}";
    }

    private static string BuildProveedor(Persistence.Entities.ProveedorEntity proveedor)
    {
        return proveedor.Persona.RazonSocialONombresCompletos;
    }

    private static NaturalezaCompra ToDomainNaturaleza(SharedNaturalezaCompra naturalezaCompra) => naturalezaCompra switch
    {
        SharedNaturalezaCompra.MercaderiaInventario => NaturalezaCompra.MercaderiaInventario,
        SharedNaturalezaCompra.ActivoFijo => NaturalezaCompra.ActivoFijo,
        SharedNaturalezaCompra.GastoServicio => NaturalezaCompra.GastoServicio,
        _ => throw new InvalidOperationException("La naturaleza de compra seleccionada no es valida.")
    };
}
