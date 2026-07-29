using Microsoft.EntityFrameworkCore;
using TestDeIa.Application.Modules.Dashboard.Ports.Out;
using TestDeIa.Domain.Modules.Dashboard.Entities;
using TestDeIa.Domain.Modules.Facturacion.Entities;
using TestDeIa.Infrastructure.Persistence;
using TestDeIa.Shared.Responses.Dashboard;
using TestDeIa.Shared.Security;

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

    public async Task<decimal> GetTotalVentasAsync(DateTimeOffset periodoInicio, DateTimeOffset periodoFin, CancellationToken cancellationToken = default)
    {
        var total = await dbContext.Facturas
            .AsNoTracking()
            .Where(current =>
                current.FechaEmision >= periodoInicio &&
                current.FechaEmision < periodoFin &&
                (current.Estado == FacturaEstado.AUTORIZADO || current.Estado == FacturaEstado.PENDIENTE))
            .SumAsync(current => (decimal?)current.Total, cancellationToken) ?? 0m;

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<IReadOnlyCollection<DashboardProductividadUsuarioResponse>> GetProductividadUsuariosAsync(
        DateTimeOffset periodoInicio,
        DateTimeOffset periodoFin,
        int take,
        CancellationToken cancellationToken = default)
    {
        var usuariosOperativos = await dbContext.SecurityUsers
            .AsNoTracking()
            .Where(current =>
                current.IsActive &&
                current.UserRoles.Any(userRole =>
                    userRole.Role.Name == SecurityRoleNames.Cajero ||
                    userRole.Role.Name == SecurityRoleNames.AsesorComercial))
            .Select(current => new
            {
                current.Id,
                Usuario = current.DisplayName
            })
            .ToListAsync(cancellationToken);

        if (usuariosOperativos.Count == 0)
        {
            return Array.Empty<DashboardProductividadUsuarioResponse>();
        }

        var usuarioIds = usuariosOperativos.Select(current => current.Id).ToArray();
        var ventasPorUsuario = await dbContext.Facturas
            .AsNoTracking()
            .Where(current =>
                usuarioIds.Contains(current.UsuarioId) &&
                current.FechaEmision >= periodoInicio &&
                current.FechaEmision < periodoFin &&
                (current.Estado == FacturaEstado.AUTORIZADO || current.Estado == FacturaEstado.PENDIENTE))
            .GroupBy(current => current.UsuarioId)
            .Select(grouped => new
            {
                UsuarioId = grouped.Key,
                Comprobantes = grouped.Count(),
                TotalVentas = grouped.Sum(current => current.Total)
            })
            .ToDictionaryAsync(current => current.UsuarioId, cancellationToken);

        return usuariosOperativos
            .Select(usuario =>
            {
                ventasPorUsuario.TryGetValue(usuario.Id, out var ventas);
                return new DashboardProductividadUsuarioResponse
                {
                    UsuarioId = usuario.Id,
                    Usuario = usuario.Usuario,
                    Comprobantes = ventas?.Comprobantes ?? 0,
                    TotalVentas = Math.Round(ventas?.TotalVentas ?? 0m, 2, MidpointRounding.AwayFromZero)
                };
            })
            .OrderByDescending(current => current.TotalVentas)
            .ThenBy(current => current.Usuario)
            .Take(take)
            .ToArray();
    }

    public async Task<DashboardCajeroOverviewResponse> GetCajeroOverviewAsync(
        Guid usuarioId,
        DateTimeOffset periodoInicio,
        DateTimeOffset periodoFin,
        CancellationToken cancellationToken = default)
    {
        var ventas = await dbContext.Facturas
            .AsNoTracking()
            .Where(current =>
                current.UsuarioId == usuarioId &&
                current.FechaEmision >= periodoInicio &&
                current.FechaEmision < periodoFin &&
                (current.Estado == FacturaEstado.AUTORIZADO || current.Estado == FacturaEstado.PENDIENTE))
            .OrderByDescending(current => current.FechaEmision)
            .Select(current => new DashboardVentaCajeroResponse
            {
                FacturaId = current.Id,
                NumeroComprobante = $"{current.Establecimiento}-{current.PuntoEmision}-{current.Secuencial.ToString().PadLeft(9, '0')}",
                FechaEmision = current.FechaEmision,
                ClienteNombre = current.ClienteNombre,
                FormaPago = current.FormaPago,
                Total = current.Total,
                Estado = current.Estado.ToString()
            })
            .ToListAsync(cancellationToken);

        var formaPagoMasUsada = ventas
            .GroupBy(current => string.IsNullOrWhiteSpace(current.FormaPago) ? "No especificada" : current.FormaPago)
            .OrderByDescending(current => current.Count())
            .ThenByDescending(current => current.Sum(item => item.Total))
            .Select(current => current.Key)
            .FirstOrDefault() ?? "Sin movimientos";

        return new DashboardCajeroOverviewResponse
        {
            VentasDia = Math.Round(ventas.Sum(current => current.Total), 2, MidpointRounding.AwayFromZero),
            ComprobantesEmitidos = ventas.Count,
            FormaPagoMasUsada = formaPagoMasUsada,
            UltimasVentas = ventas.Take(10).ToArray()
        };
    }

    public async Task<DashboardBodegueroOverviewResponse> GetBodegueroOverviewAsync(
        DateTimeOffset diaInicio,
        DateTimeOffset diaFin,
        DateTimeOffset mesInicio,
        DateTimeOffset mesFin,
        CancellationToken cancellationToken = default)
    {
        var bodegas = await dbContext.Bodegas
            .AsNoTracking()
            .Where(current => current.IsActive)
            .OrderByDescending(current => current.EsPrincipal)
            .ThenBy(current => current.Nombre)
            .Select(current => new
            {
                current.Id,
                current.Codigo,
                current.Nombre,
                current.EsPrincipal
            })
            .ToListAsync(cancellationToken);

        if (bodegas.Count == 0)
        {
            return new DashboardBodegueroOverviewResponse();
        }

        var bodegaIds = bodegas.Select(current => current.Id).ToArray();
        var stockPorBodega = await dbContext.ProductosBodega
            .AsNoTracking()
            .Include(current => current.Producto)
            .Where(current => bodegaIds.Contains(current.BodegaId) && current.Producto.IsActive && current.Producto.ControlaStock)
            .GroupBy(current => current.BodegaId)
            .Select(grouped => new
            {
                BodegaId = grouped.Key,
                ItemsConStock = grouped.Count(current => current.StockActual > 0),
                ItemsCriticos = grouped.Count(current => current.StockActual <= (current.Producto.StockMinimo ?? 0m)),
                StockTotal = grouped.Sum(current => current.StockActual),
                ValorInventario = grouped.Sum(current => current.StockActual * current.Producto.CostoPromedio)
            })
            .ToDictionaryAsync(current => current.BodegaId, cancellationToken);

        var movimientosMes = await dbContext.KardexMovimientos
            .AsNoTracking()
            .Where(current =>
                bodegaIds.Contains(current.BodegaId) &&
                current.FechaMovimiento >= mesInicio &&
                current.FechaMovimiento < mesFin)
            .Select(current => new
            {
                current.ProductoId,
                current.BodegaId,
                current.Producto.Codigo,
                current.Producto.Nombre,
                BodegaNombre = current.Bodega.Nombre,
                current.FechaMovimiento,
                current.CantidadEntrada,
                current.CantidadSalida
            })
            .ToListAsync(cancellationToken);

        var movimientosPorBodega = movimientosMes
            .GroupBy(current => current.BodegaId)
            .ToDictionary(
                current => current.Key,
                current => new
                {
                    EntradasDia = current.Where(item => item.FechaMovimiento >= diaInicio && item.FechaMovimiento < diaFin).Sum(item => item.CantidadEntrada),
                    SalidasDia = current.Where(item => item.FechaMovimiento >= diaInicio && item.FechaMovimiento < diaFin).Sum(item => item.CantidadSalida),
                    EntradasMes = current.Sum(item => item.CantidadEntrada),
                    SalidasMes = current.Sum(item => item.CantidadSalida)
                });

        var bodegaEstados = bodegas
            .Select(bodega =>
            {
                stockPorBodega.TryGetValue(bodega.Id, out var stock);
                movimientosPorBodega.TryGetValue(bodega.Id, out var movimientos);

                return new DashboardBodegaEstadoResponse
                {
                    BodegaId = bodega.Id,
                    Codigo = bodega.Codigo,
                    Nombre = bodega.Nombre,
                    EsPrincipal = bodega.EsPrincipal,
                    ItemsConStock = stock?.ItemsConStock ?? 0,
                    ItemsCriticos = stock?.ItemsCriticos ?? 0,
                    StockTotal = Math.Round(stock?.StockTotal ?? 0m, 4, MidpointRounding.AwayFromZero),
                    ValorInventario = Math.Round(stock?.ValorInventario ?? 0m, 2, MidpointRounding.AwayFromZero),
                    EntradasDia = Math.Round(movimientos?.EntradasDia ?? 0m, 4, MidpointRounding.AwayFromZero),
                    SalidasDia = Math.Round(movimientos?.SalidasDia ?? 0m, 4, MidpointRounding.AwayFromZero),
                    EntradasMes = Math.Round(movimientos?.EntradasMes ?? 0m, 4, MidpointRounding.AwayFromZero),
                    SalidasMes = Math.Round(movimientos?.SalidasMes ?? 0m, 4, MidpointRounding.AwayFromZero)
                };
            })
            .ToArray();

        var itemsMovilizados = movimientosMes
            .GroupBy(current => new
            {
                current.ProductoId,
                current.BodegaId,
                current.Codigo,
                current.Nombre,
                current.BodegaNombre
            })
            .Select(grouped =>
            {
                var entradasDia = grouped.Where(item => item.FechaMovimiento >= diaInicio && item.FechaMovimiento < diaFin).Sum(item => item.CantidadEntrada);
                var salidasDia = grouped.Where(item => item.FechaMovimiento >= diaInicio && item.FechaMovimiento < diaFin).Sum(item => item.CantidadSalida);
                var entradasMes = grouped.Sum(item => item.CantidadEntrada);
                var salidasMes = grouped.Sum(item => item.CantidadSalida);

                return new DashboardBodegaItemMovimientoResponse
                {
                    ProductoId = grouped.Key.ProductoId,
                    BodegaId = grouped.Key.BodegaId,
                    CodigoProducto = grouped.Key.Codigo,
                    NombreProducto = grouped.Key.Nombre,
                    BodegaNombre = grouped.Key.BodegaNombre,
                    EntradasDia = Math.Round(entradasDia, 4, MidpointRounding.AwayFromZero),
                    SalidasDia = Math.Round(salidasDia, 4, MidpointRounding.AwayFromZero),
                    EntradasMes = Math.Round(entradasMes, 4, MidpointRounding.AwayFromZero),
                    SalidasMes = Math.Round(salidasMes, 4, MidpointRounding.AwayFromZero),
                    MovimientoTotalMes = Math.Round(entradasMes + salidasMes, 4, MidpointRounding.AwayFromZero)
                };
            })
            .OrderByDescending(current => current.MovimientoTotalMes)
            .ThenBy(current => current.NombreProducto)
            .Take(12)
            .ToArray();

        return new DashboardBodegueroOverviewResponse
        {
            EntradasDia = Math.Round(bodegaEstados.Sum(current => current.EntradasDia), 4, MidpointRounding.AwayFromZero),
            SalidasDia = Math.Round(bodegaEstados.Sum(current => current.SalidasDia), 4, MidpointRounding.AwayFromZero),
            EntradasMes = Math.Round(bodegaEstados.Sum(current => current.EntradasMes), 4, MidpointRounding.AwayFromZero),
            SalidasMes = Math.Round(bodegaEstados.Sum(current => current.SalidasMes), 4, MidpointRounding.AwayFromZero),
            BodegasActivas = bodegaEstados.Length,
            ItemsCriticos = bodegaEstados.Sum(current => current.ItemsCriticos),
            ValorInventario = Math.Round(bodegaEstados.Sum(current => current.ValorInventario), 2, MidpointRounding.AwayFromZero),
            Bodegas = bodegaEstados,
            ItemsMovilizados = itemsMovilizados
        };
    }
}
