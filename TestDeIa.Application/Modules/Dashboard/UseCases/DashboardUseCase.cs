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
        var periodoPrediccionInicio = periodoInicio.AddDays(-60);

        var resumen = await dashboardAnalyticsRepository.GetResumenMensualAsync(periodoInicio, periodoFin, cancellationToken);
        var topProductos = await dashboardAnalyticsRepository.GetTopProductosVendidosAsync(periodoInicio, periodoFin, 5, cancellationToken);
        var historial = await dashboardAnalyticsRepository.GetConsumoHistoricoAsync(periodoPrediccionInicio, periodoFin, 15, cancellationToken);
        var alertas = await inventarioPredictivoService.PredecirAlertasAsync(historial, cancellationToken);

        return new DashboardOverviewResponse
        {
            ResumenFinanciero = new DashboardFinancialSummaryResponse
            {
                TotalVentasFacturadas = resumen.TotalVentasFacturadas,
                TotalComprasRegistradas = resumen.TotalComprasRegistradas,
                MargenGananciaEstimado = resumen.MargenGananciaEstimado
            },
            TopProductos = topProductos.Select(MapTopProducto).ToArray(),
            AlertasPredictivas = alertas.Select(MapAlerta).ToArray()
        };
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
}
