using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Dashboard.Ports.Out;
using TestDeIa.Domain.Modules.Dashboard.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;

namespace TestDeIa.Infrastructure.Adapters.Out.Dashboard;

public sealed class EfDashboardAnalyticsRepository : IDashboardAnalyticsRepository
{
    private readonly TestDeIaDbContext dbContext;

    public EfDashboardAnalyticsRepository(TestDeIaDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<DashboardResumenFinanciero> GetResumenMensualAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, CancellationToken cancellationToken = default)
    {
        var ventasQuery = dbContext.Facturas
            .AsNoTracking()
            .Where(current =>
                current.FechaEmision >= periodoInicio &&
                current.FechaEmision < periodoFin &&
                (current.Estado == FacturaEstado.AUTORIZADO || current.Estado == FacturaEstado.PENDIENTE));

        var comprasQuery = dbContext.Compras
            .AsNoTracking()
            .Where(current =>
                current.FechaEmision >= periodoInicio &&
                current.FechaEmision < periodoFin &&
                current.EstadoCompra == "Registrada");

        var totalVentas = await ventasQuery.SumAsync(current => (decimal?)current.Total, cancellationToken) ?? 0m;
        var totalCompras = await comprasQuery.SumAsync(current => (decimal?)current.ImporteTotal, cancellationToken) ?? 0m;

        var detallesVentasQuery =
            from factura in ventasQuery
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            select new { detalle, producto };

        var ventasInventario = await detallesVentasQuery
            .Where(current => current.producto.ControlaStock)
            .SumAsync(current => (decimal?)current.detalle.Total, cancellationToken) ?? 0m;

        var ingresosPorServicios = await detallesVentasQuery
            .Where(current => !current.producto.ControlaStock)
            .SumAsync(current => (decimal?)current.detalle.Total, cancellationToken) ?? 0m;

        var costoVentas = await detallesVentasQuery
            .Where(current => current.producto.ControlaStock)
            .SumAsync(current => (decimal?)(current.detalle.Cantidad * current.producto.CostoPromedio), cancellationToken) ?? 0m;

        return new DashboardResumenFinanciero
        {
            TotalVentasFacturadas = Math.Round(totalVentas, 2, MidpointRounding.AwayFromZero),
            VentasInventario = Math.Round(ventasInventario, 2, MidpointRounding.AwayFromZero),
            IngresosPorServicios = Math.Round(ingresosPorServicios, 2, MidpointRounding.AwayFromZero),
            TotalComprasRegistradas = Math.Round(totalCompras, 2, MidpointRounding.AwayFromZero),
            MargenGananciaEstimado = Math.Round(totalVentas - costoVentas, 2, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<IReadOnlyCollection<DashboardTopProducto>> GetTopProductosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default)
    {
        var items = await (
            from factura in dbContext.Facturas.AsNoTracking()
            where factura.FechaEmision >= periodoInicio &&
                  factura.FechaEmision < periodoFin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            group new { detalle, producto } by new
            {
                detalle.ProductoId,
                detalle.CodigoProducto,
                detalle.NombreProducto
            } into grouped
            orderby grouped.Sum(current => current.detalle.Cantidad) descending,
                    grouped.Sum(current => current.detalle.Total) descending
            select new DashboardTopProducto
            {
                ProductoId = grouped.Key.ProductoId,
                Codigo = grouped.Key.CodigoProducto,
                Nombre = grouped.Key.NombreProducto,
                CantidadVendida = Math.Round(grouped.Sum(current => current.detalle.Cantidad), 2, MidpointRounding.AwayFromZero),
                TotalVendido = Math.Round(grouped.Sum(current => current.detalle.Total), 2, MidpointRounding.AwayFromZero),
                CostoEstimado = Math.Round(grouped.Sum(current => current.detalle.Cantidad * current.producto.CostoPromedio), 2, MidpointRounding.AwayFromZero),
                MargenEstimado = Math.Round(
                    grouped.Sum(current => current.detalle.Total) -
                    grouped.Sum(current => current.detalle.Cantidad * current.producto.CostoPromedio),
                    2,
                    MidpointRounding.AwayFromZero)
            })
            .Take(take)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyCollection<DashboardTopProducto>> GetTopServiciosVendidosAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int take, CancellationToken cancellationToken = default)
    {
        var items = await (
            from factura in dbContext.Facturas.AsNoTracking()
            where factura.FechaEmision >= periodoInicio &&
                  factura.FechaEmision < periodoFin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            from detalle in factura.Detalles
            join producto in dbContext.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            where !producto.ControlaStock
            group new { detalle, producto } by new
            {
                detalle.ProductoId,
                detalle.CodigoProducto,
                detalle.NombreProducto
            } into grouped
            orderby grouped.Sum(current => current.detalle.Total) descending,
                    grouped.Sum(current => current.detalle.Cantidad) descending
            select new DashboardTopProducto
            {
                ProductoId = grouped.Key.ProductoId,
                Codigo = grouped.Key.CodigoProducto,
                Nombre = grouped.Key.NombreProducto,
                CantidadVendida = Math.Round(grouped.Sum(current => current.detalle.Cantidad), 2, MidpointRounding.AwayFromZero),
                TotalVendido = Math.Round(grouped.Sum(current => current.detalle.Total), 2, MidpointRounding.AwayFromZero),
                CostoEstimado = 0m,
                MargenEstimado = Math.Round(grouped.Sum(current => current.detalle.Total), 2, MidpointRounding.AwayFromZero)
            })
            .Take(take)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyCollection<ProductoBodegaConsumoHistorico>> GetConsumoHistoricoAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, int topProductos, CancellationToken cancellationToken = default)
    {
        var productIds = await (
            from factura in dbContext.Facturas.AsNoTracking()
            where factura.FechaEmision >= periodoInicio &&
                  factura.FechaEmision < periodoFin &&
                  (factura.Estado == FacturaEstado.AUTORIZADO || factura.Estado == FacturaEstado.PENDIENTE)
            from detalle in factura.Detalles
            group detalle by detalle.ProductoId into grouped
            orderby grouped.Sum(current => current.Cantidad) descending
            select grouped.Key)
            .Take(topProductos)
            .ToListAsync(cancellationToken);

        if (productIds.Count == 0)
        {
            return Array.Empty<ProductoBodegaConsumoHistorico>();
        }

        var existencias = await dbContext.ProductosBodega
            .AsNoTracking()
            .Include(current => current.Producto)
            .Include(current => current.Bodega)
            .Where(current =>
                productIds.Contains(current.ProductoId) &&
                current.Producto.IsActive &&
                current.Producto.ControlaStock &&
                current.Bodega.IsActive)
            .ToListAsync(cancellationToken);

        if (existencias.Count == 0)
        {
            return Array.Empty<ProductoBodegaConsumoHistorico>();
        }

        var movements = await dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(current =>
                productIds.Contains(current.ProductoId) &&
                current.FechaMovimiento >= periodoInicio &&
                current.FechaMovimiento < periodoFin &&
                current.TipoMovimiento == "Salida" &&
                current.Concepto.Contains("Factura"))
            .ToListAsync(cancellationToken);

        return existencias
            .Select(existencia =>
            {
                var consumos = movements
                    .Where(current => current.ProductoId == existencia.ProductoId && current.BodegaId == existencia.BodegaId)
                    .GroupBy(current => DateOnly.FromDateTime(current.FechaMovimiento.LocalDateTime.Date))
                    .OrderBy(current => current.Key)
                    .Select(current => new ConsumoDiarioHistorico
                    {
                        Fecha = current.Key,
                        Cantidad = Math.Round(current.Sum(item => item.CantidadSalida), 4, MidpointRounding.AwayFromZero)
                    })
                    .ToArray();

                return new ProductoBodegaConsumoHistorico
                {
                    ProductoId = existencia.ProductoId,
                    BodegaId = existencia.BodegaId,
                    CodigoProducto = existencia.Producto.Codigo,
                    NombreProducto = existencia.Producto.Nombre,
                    BodegaNombre = existencia.Bodega.Nombre,
                    StockActual = existencia.StockActual,
                    StockMinimo = existencia.Producto.StockMinimo ?? 0,
                    CostoPromedio = existencia.Producto.CostoPromedio,
                    ConsumosDiarios = consumos
                };
            })
            .ToArray();
    }
}
