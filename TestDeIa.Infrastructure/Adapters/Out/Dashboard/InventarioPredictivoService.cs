using Microsoft.ML;
using TestDeIa.Application.Modules.Dashboard.Ports.Out;
using TestDeIa.Domain.Modules.Dashboard.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Dashboard;

public sealed class InventarioPredictivoService : IInventarioPredictivoService
{
    private const decimal MinimoConsumoSignificativo = 0.05m;
    private readonly MLContext mlContext = new(seed: 42);

    public Task<IReadOnlyCollection<DashboardAlertaPredictivaStock>> PredecirAlertasAsync(
        IReadOnlyCollection<ProductoBodegaConsumoHistorico> historial,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var today = DateOnly.FromDateTime(DateTime.Now.Date);

        var alertas = historial
            .Select(item => BuildAlert(item, today))
            .Where(current => current is not null)
            .Select(current => current!)
            .OrderBy(current => current.DiasStockEstimados)
            .ThenBy(current => current.StockActual)
            .Take(12)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<DashboardAlertaPredictivaStock>>(alertas);
    }

    private DashboardAlertaPredictivaStock? BuildAlert(ProductoBodegaConsumoHistorico item, DateOnly today)
    {
        var timeline = NormalizeTimeline(item.ConsumosDiarios, today);
        var averageRecent = timeline.Count == 0
            ? 0m
            : Math.Round((decimal)timeline.TakeLast(Math.Min(7, timeline.Count)).Average(current => current.Quantity), 4, MidpointRounding.AwayFromZero);

        var predictedDaily = timeline.Count >= 6
            ? PredictDailyConsumption(timeline, averageRecent)
            : averageRecent;

        if (predictedDaily <= 0m && item.StockActual > 0m)
        {
            return item.StockActual <= item.StockMinimo
                ? new DashboardAlertaPredictivaStock
                {
                    ProductoId = item.ProductoId,
                    BodegaId = item.BodegaId,
                    CodigoProducto = item.CodigoProducto,
                    NombreProducto = item.NombreProducto,
                    BodegaNombre = item.BodegaNombre,
                    StockActual = item.StockActual,
                    StockMinimo = item.StockMinimo,
                    ConsumoPromedioDiario = averageRecent,
                    TendenciaConsumo = 0m,
                    DiasStockEstimados = 999m,
                    FechaProbableQuiebre = null,
                    Riesgo = "Observacion"
                }
                : null;
        }

        var effectiveConsumption = Math.Max(predictedDaily, averageRecent);
        if (effectiveConsumption < MinimoConsumoSignificativo)
        {
            effectiveConsumption = averageRecent;
        }

        if (effectiveConsumption < MinimoConsumoSignificativo)
        {
            return item.StockActual <= item.StockMinimo
                ? new DashboardAlertaPredictivaStock
                {
                    ProductoId = item.ProductoId,
                    BodegaId = item.BodegaId,
                    CodigoProducto = item.CodigoProducto,
                    NombreProducto = item.NombreProducto,
                    BodegaNombre = item.BodegaNombre,
                    StockActual = item.StockActual,
                    StockMinimo = item.StockMinimo,
                    ConsumoPromedioDiario = 0m,
                    TendenciaConsumo = 0m,
                    DiasStockEstimados = 999m,
                    FechaProbableQuiebre = null,
                    Riesgo = "Observacion"
                }
                : null;
        }

        var daysRemaining = item.StockActual <= 0
            ? 0m
            : Math.Round(item.StockActual / effectiveConsumption, 1, MidpointRounding.AwayFromZero);

        if (daysRemaining > 21m && item.StockActual > item.StockMinimo)
        {
            return null;
        }

        var breakDate = daysRemaining <= 0m
            ? today
            : today.AddDays((int)Math.Ceiling((double)daysRemaining));

        return new DashboardAlertaPredictivaStock
        {
            ProductoId = item.ProductoId,
            BodegaId = item.BodegaId,
            CodigoProducto = item.CodigoProducto,
            NombreProducto = item.NombreProducto,
            BodegaNombre = item.BodegaNombre,
            StockActual = item.StockActual,
            StockMinimo = item.StockMinimo,
            ConsumoPromedioDiario = Math.Round(averageRecent, 2, MidpointRounding.AwayFromZero),
            TendenciaConsumo = Math.Round(effectiveConsumption, 2, MidpointRounding.AwayFromZero),
            DiasStockEstimados = daysRemaining,
            FechaProbableQuiebre = breakDate,
            Riesgo = ResolveRisk(daysRemaining, item.StockActual, item.StockMinimo)
        };
    }

    private decimal PredictDailyConsumption(IReadOnlyList<DailyConsumptionSample> timeline, decimal averageRecent)
    {
        var data = timeline.Select(current => new ConsumptionTrainingRow
        {
            DayIndex = current.DayIndex,
            DayOfWeek = current.DayOfWeek,
            RollingAverage = current.RollingAverage,
            Quantity = current.Quantity
        });

        var dataView = mlContext.Data.LoadFromEnumerable(data);
        var pipeline = mlContext.Transforms.Concatenate(
                "Features",
                nameof(ConsumptionTrainingRow.DayIndex),
                nameof(ConsumptionTrainingRow.DayOfWeek),
                nameof(ConsumptionTrainingRow.RollingAverage))
            .Append(mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(mlContext.Regression.Trainers.Sdca(
                labelColumnName: nameof(ConsumptionTrainingRow.Quantity),
                featureColumnName: "Features"));

        var model = pipeline.Fit(dataView);
        var engine = mlContext.Model.CreatePredictionEngine<ConsumptionTrainingRow, ConsumptionPrediction>(model);

        var predictions = new List<decimal>();
        var last = timeline[^1];
        for (var future = 1; future <= 7; future++)
        {
            var score = engine.Predict(new ConsumptionTrainingRow
            {
                DayIndex = last.DayIndex + future,
                DayOfWeek = ((last.DayOfWeek + future - 1) % 7) + 1,
                RollingAverage = averageRecent <= 0 ? last.RollingAverage : (float)averageRecent
            }).Score;

            predictions.Add(Math.Max(0m, Math.Round((decimal)score, 4, MidpointRounding.AwayFromZero)));
        }

        if (predictions.Count == 0)
        {
            return averageRecent;
        }

        return Math.Round(predictions.Average(), 4, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyList<DailyConsumptionSample> NormalizeTimeline(IReadOnlyCollection<ConsumoDiarioHistorico> consumos, DateOnly today)
    {
        if (consumos.Count == 0)
        {
            return Array.Empty<DailyConsumptionSample>();
        }

        var firstDay = consumos.Min(current => current.Fecha);
        var lastDay = consumos.Max(current => current.Fecha);
        if (lastDay < today.AddDays(-1))
        {
            lastDay = today.AddDays(-1);
        }

        var map = consumos.ToDictionary(current => current.Fecha, current => current.Cantidad);
        var timeline = new List<DailyConsumptionSample>();
        var rollingWindow = new Queue<decimal>();

        for (var day = firstDay; day <= lastDay; day = day.AddDays(1))
        {
            var quantity = map.GetValueOrDefault(day);
            rollingWindow.Enqueue(quantity);
            if (rollingWindow.Count > 7)
            {
                rollingWindow.Dequeue();
            }

            timeline.Add(new DailyConsumptionSample
            {
                DayIndex = timeline.Count + 1,
                DayOfWeek = (int)day.DayOfWeek + 1,
                Quantity = (float)quantity,
                RollingAverage = (float)rollingWindow.Average()
            });
        }

        return timeline;
    }

    private static string ResolveRisk(decimal daysRemaining, decimal stockActual, decimal stockMinimo)
    {
        if (stockActual <= 0 || daysRemaining <= 3m)
        {
            return "Critico";
        }

        if (stockActual <= stockMinimo || daysRemaining <= 7m)
        {
            return "Alto";
        }

        if (daysRemaining <= 14m)
        {
            return "Medio";
        }

        return "Bajo";
    }

    private sealed class DailyConsumptionSample
    {
        public float DayIndex { get; init; }
        public float DayOfWeek { get; init; }
        public float Quantity { get; init; }
        public float RollingAverage { get; init; }
    }

    private sealed class ConsumptionTrainingRow
    {
        public float DayIndex { get; init; }
        public float DayOfWeek { get; init; }
        public float RollingAverage { get; init; }
        public float Quantity { get; init; }
    }

    private sealed class ConsumptionPrediction
    {
        public float Score { get; init; }
    }
}
