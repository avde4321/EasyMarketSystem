using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Reporteria.Ports.In;
using TestDeIa.Domain.Modules.Caja.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Domain.Modules.Inventario;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Reporteria;
using TestDeIa.Shared.Sri;

namespace TestDeIa.Infrastructure.Adapters.Out.Reporteria;

public sealed class ConsolidacionContableService(TestDeIaDbContext dbContext) : IConsolidacionContableService
{
    public async Task<ConsolidacionContableDiariaResponse> ObtenerResumenDiarioAsync(Guid empresaId, DateTime fecha, CancellationToken cancellationToken = default)
    {
        var inicio = new DateTimeOffset(fecha.Date, TimeSpan.Zero);
        var fin = inicio.AddDays(1);

        var ventas = await dbContext.Facturas
            .AsNoTracking()
            .Where(factura =>
                factura.EmpresaId == empresaId &&
                factura.FechaEmision >= inicio &&
                factura.FechaEmision < fin &&
                (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE))
            .GroupBy(_ => 1)
            .Select(group => new
            {
                VentasNetas = group.Sum(factura => factura.Subtotal),
                IvaVentas = group.Sum(factura => factura.IvaTotal)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var pagos = await (
            from pago in dbContext.FacturaPagos.AsNoTracking()
            join factura in dbContext.Facturas.AsNoTracking() on pago.FacturaId equals factura.Id
            where pago.EmpresaId == empresaId &&
                  factura.FechaEmision >= inicio &&
                  factura.FechaEmision < fin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            group pago by pago.FormaPagoCodigo into grouped
            select new
            {
                FormaPagoCodigo = grouped.Key,
                Total = grouped.Sum(item => item.Monto)
            })
            .ToArrayAsync(cancellationToken);

        var cajaBancos = pagos
            .Where(pago => pago.FormaPagoCodigo is SriCatalogCodes.FormaPagoEfectivo or SriCatalogCodes.FormaPagoTransferencia or SriCatalogCodes.FormaPagoTarjetaCredito or SriCatalogCodes.FormaPagoTarjetaDebito)
            .Sum(pago => pago.Total);

        var costoVentas = await dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(movimiento =>
                movimiento.EmpresaId == empresaId &&
                movimiento.FechaMovimiento >= inicio &&
                movimiento.FechaMovimiento < fin &&
                movimiento.TipoMovimiento == TipoMovimientoInventario.SalidaVenta)
            .SumAsync(movimiento => (decimal?)movimiento.CostoTotal, cancellationToken) ?? 0m;

        var ajustesInventario = await dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(movimiento =>
                movimiento.EmpresaId == empresaId &&
                movimiento.FechaMovimiento >= inicio &&
                movimiento.FechaMovimiento < fin &&
                (movimiento.TipoMovimiento == TipoMovimientoInventario.AjusteEgreso ||
                 movimiento.TipoMovimiento == TipoMovimientoInventario.MermaInventario))
            .SumAsync(movimiento => (decimal?)movimiento.CostoTotal, cancellationToken) ?? 0m;

        var ajustesCaja = await dbContext.CajaSesiones
            .AsNoTracking()
            .Where(caja =>
                caja.EmpresaId == empresaId &&
                caja.FechaCierre >= inicio &&
                caja.FechaCierre < fin &&
                caja.EstadoCaja == CajaEstado.Cerrada)
            .SumAsync(caja => (decimal?)caja.DiferenciaMonto, cancellationToken) ?? 0m;

        return new ConsolidacionContableDiariaResponse
        {
            Fecha = fecha.Date,
            VentasNetas = Math.Round(ventas?.VentasNetas ?? 0m, 2),
            IvaVentas = Math.Round(ventas?.IvaVentas ?? 0m, 2),
            CostoVentas = Math.Round(costoVentas, 2),
            MovimientoInventario = Math.Round(costoVentas + ajustesInventario, 2),
            CajaBancos = Math.Round(cajaBancos, 2),
            CuentasPorCobrar = 0m,
            AjustesCaja = Math.Round(ajustesCaja, 2),
            MermasInventario = Math.Round(ajustesInventario, 2)
        };
    }
}
