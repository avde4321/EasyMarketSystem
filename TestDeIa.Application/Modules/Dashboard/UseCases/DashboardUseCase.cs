using TestDeIa.Application.Modules.Dashboard.Ports.In;
using TestDeIa.Application.Modules.Dashboard.Ports.Out;
using TestDeIa.Domain.Modules.Dashboard.Entities;
using TestDeIa.Shared.Responses.Dashboard;

namespace TestDeIa.Application.Modules.Dashboard.UseCases;

public sealed class DashboardUseCase : IDashboardUseCase
{
    private readonly IDashboardAnalyticsRepository dashboardAnalyticsRepository;
    private readonly IInventarioPredictivoService inventarioPredictivoService;

    public DashboardUseCase(
        IDashboardAnalyticsRepository dashboardAnalyticsRepository,
        IInventarioPredictivoService inventarioPredictivoService)
    {
        this.dashboardAnalyticsRepository = dashboardAnalyticsRepository;
        this.inventarioPredictivoService = inventarioPredictivoService;
    }

    public async Task<DashboardOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now;
        var periodoInicio = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
        var periodoFin = periodoInicio.AddMonths(1);
        var periodoInteranualInicio = periodoInicio.AddYears(-1);
        var periodoInteranualFin = periodoFin.AddYears(-1);
        var periodoPrediccionInicio = periodoInicio.AddDays(-60);

        var resumen = await dashboardAnalyticsRepository.GetResumenMensualAsync(periodoInicio, periodoFin, cancellationToken);
        var topProductos = await dashboardAnalyticsRepository.GetTopProductosVendidosAsync(periodoInicio, periodoFin, 5, cancellationToken);
        var topServicios = await dashboardAnalyticsRepository.GetTopServiciosVendidosAsync(periodoInicio, periodoFin, 5, cancellationToken);
        var historial = await dashboardAnalyticsRepository.GetConsumoHistoricoAsync(periodoPrediccionInicio, periodoFin, 15, cancellationToken);
        var alertas = await inventarioPredictivoService.PredecirAlertasAsync(historial, cancellationToken);
        var ventasInteranual = await dashboardAnalyticsRepository.GetTotalVentasAsync(periodoInteranualInicio, periodoInteranualFin, cancellationToken);
        var productividad = await dashboardAnalyticsRepository.GetProductividadUsuariosAsync(periodoInicio, periodoFin, 8, cancellationToken);

        return new DashboardOverviewResponse
        {
            ResumenFinanciero = new DashboardFinancialSummaryResponse
            {
                TotalVentasFacturadas = resumen.TotalVentasFacturadas,
                VentasInventario = resumen.VentasInventario,
                IngresosPorServicios = resumen.IngresosPorServicios,
                TotalComprasRegistradas = resumen.TotalComprasRegistradas,
                MargenGananciaEstimado = resumen.MargenGananciaEstimado
            },
            TopProductos = topProductos.Select(MapTopProducto).ToArray(),
            TopServicios = topServicios.Select(MapTopProducto).ToArray(),
            AlertasPredictivas = alertas.Select(MapAlerta).ToArray(),
            ComparativoInteranual = new DashboardVentasComparativoResponse
            {
                MesActual = resumen.TotalVentasFacturadas,
                MismoMesAnioAnterior = ventasInteranual,
                PorcentajeVariacion = CalculateVariation(resumen.TotalVentasFacturadas, ventasInteranual)
            },
            ProductividadUsuarios = productividad,
            EstrategiasNegocio = BuildEstrategias(topProductos, alertas)
        };
    }

    public Task<DashboardCajeroOverviewResponse> GetCajeroOverviewAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now;
        var periodoInicio = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var periodoFin = periodoInicio.AddDays(1);

        return dashboardAnalyticsRepository.GetCajeroOverviewAsync(usuarioId, periodoInicio, periodoFin, cancellationToken);
    }

    private static DashboardTopProductoResponse MapTopProducto(DashboardTopProducto producto)
    {
        return new DashboardTopProductoResponse
        {
            ProductoId = producto.ProductoId,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            CantidadVendida = producto.CantidadVendida,
            TotalVendido = producto.TotalVendido,
            CostoEstimado = producto.CostoEstimado,
            MargenEstimado = producto.MargenEstimado
        };
    }

    private static DashboardStockPredictionAlertResponse MapAlerta(DashboardAlertaPredictivaStock alerta)
    {
        return new DashboardStockPredictionAlertResponse
        {
            ProductoId = alerta.ProductoId,
            BodegaId = alerta.BodegaId,
            CodigoProducto = alerta.CodigoProducto,
            NombreProducto = alerta.NombreProducto,
            BodegaNombre = alerta.BodegaNombre,
            StockActual = alerta.StockActual,
            StockMinimo = alerta.StockMinimo,
            ConsumoPromedioDiario = alerta.ConsumoPromedioDiario,
            TendenciaConsumo = alerta.TendenciaConsumo,
            DiasStockEstimados = alerta.DiasStockEstimados,
            FechaProbableQuiebre = alerta.FechaProbableQuiebre,
            Riesgo = alerta.Riesgo
        };
    }

    private static decimal CalculateVariation(decimal actual, decimal anterior)
    {
        if (anterior == 0m)
        {
            return actual > 0m ? 100m : 0m;
        }

        return Math.Round(((actual - anterior) / anterior) * 100m, 2, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyCollection<DashboardEstrategiaNegocioResponse> BuildEstrategias(
        IReadOnlyCollection<DashboardTopProducto> topProductos,
        IReadOnlyCollection<DashboardAlertaPredictivaStock> alertas)
    {
        var estrategias = new List<DashboardEstrategiaNegocioResponse>();

        foreach (var alerta in alertas.Where(current => current.Riesgo is "Critico" or "Alto").Take(3))
        {
            estrategias.Add(new DashboardEstrategiaNegocioResponse
            {
                Titulo = "Producto con bajo stock y alta rotacion",
                Detalle = $"{alerta.NombreProducto} esta en riesgo en {alerta.BodegaNombre}. Reordenar o transferir inventario.",
                Severidad = alerta.Riesgo
            });
        }

        foreach (var producto in topProductos.Where(current => current.MargenEstimado > 0m).Take(2))
        {
            estrategias.Add(new DashboardEstrategiaNegocioResponse
            {
                Titulo = "Promocionar producto rentable",
                Detalle = $"{producto.Nombre} mantiene margen estimado de {producto.MargenEstimado:C2}. Conviene sostener exposicion comercial.",
                Severidad = "Info"
            });
        }

        if (estrategias.Count == 0)
        {
            estrategias.Add(new DashboardEstrategiaNegocioResponse
            {
                Titulo = "Operacion estable",
                Detalle = "No hay alertas criticas ni oportunidades extraordinarias con la data disponible.",
                Severidad = "Info"
            });
        }

        return estrategias;
    }
}
