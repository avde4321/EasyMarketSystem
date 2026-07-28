using System.Text.Json;
using Microsoft.ML;
using TestDeIa.Application.Modules.Compras.Models;
using TestDeIa.Application.Modules.Compras.Ports.Out;
using TestDeIa.Domain.Modules.Compras.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Compras;

public sealed class EstudioMercadoAnaliticoService : IEstudioMercadoAnaliticoService
{
    private readonly MLContext mlContext = new(seed: 84);

    public Task<EstudioMercadoAnalisisResult> GenerarAsync(
        IReadOnlyCollection<EstudioMercadoTopProducto> topProductos,
        IReadOnlyCollection<ProductoConsumoHistorico> historial,
        IReadOnlyCollection<EstudioMercadoCompra> estudiosHistoricos,
        int diasMesProximo,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var historialMap = historial.ToDictionary(current => current.ProductoId);
        var sugerencias = new List<EstudioMercadoSugerenciaCompra>(topProductos.Count);

        foreach (var topProducto in topProductos)
        {
            var productoHistorial = historialMap.GetValueOrDefault(topProducto.ProductoId);
            var demandaMensualEsperada = productoHistorial is null
                ? Math.Round(topProducto.CantidadVendida, 2, MidpointRounding.AwayFromZero)
                : ProjectMonthlyDemand(productoHistorial, diasMesProximo);

            var demandaConSeguridad = Math.Round(demandaMensualEsperada * 1.15m, 2, MidpointRounding.AwayFromZero);
            var cantidadRecomendada = Math.Max(0m, Math.Ceiling(demandaConSeguridad - topProducto.StockActual));
            var costoBase = productoHistorial?.CostoPromedio ?? 0m;

            sugerencias.Add(new EstudioMercadoSugerenciaCompra
            {
                ProductoId = topProducto.ProductoId,
                Codigo = topProducto.Codigo,
                Nombre = topProducto.Nombre,
                CantidadRecomendada = cantidadRecomendada,
                CostoEstimado = Math.Round(cantidadRecomendada * costoBase, 2, MidpointRounding.AwayFromZero),
                JustificacionAlgoritmo = BuildJustification(topProducto, demandaMensualEsperada, topProducto.StockActual, cantidadRecomendada)
            });
        }

        return Task.FromResult(new EstudioMercadoAnalisisResult
        {
            SugerenciasCompra = sugerencias,
            AnalisisEstrategicoIA = BuildStrategicAnalysis(topProductos, sugerencias, estudiosHistoricos, diasMesProximo)
        });
    }

    private decimal ProjectMonthlyDemand(ProductoConsumoHistorico historial, int diasMesProximo)
    {
        var timeline = NormalizeTimeline(historial.ConsumosDiarios);
        if (timeline.Count == 0)
        {
            return 0m;
        }

        var recentAverage = Math.Round((decimal)timeline.TakeLast(Math.Min(14, timeline.Count)).Average(current => current.Quantity), 4, MidpointRounding.AwayFromZero);
        var predictedDaily = timeline.Count >= 6
            ? PredictDailyConsumption(timeline, recentAverage)
            : recentAverage;

        return Math.Round(Math.Max(predictedDaily, recentAverage) * diasMesProximo, 2, MidpointRounding.AwayFromZero);
    }

    private decimal PredictDailyConsumption(IReadOnlyList<DailyConsumptionSample> timeline, decimal recentAverage)
    {
        var data = timeline.Select(current => new TrainingRow
        {
            DayIndex = current.DayIndex,
            DayOfWeek = current.DayOfWeek,
            RollingAverage = current.RollingAverage,
            Quantity = current.Quantity
        });

        var dataView = mlContext.Data.LoadFromEnumerable(data);
        var pipeline = mlContext.Transforms.Concatenate(
                "Features",
                nameof(TrainingRow.DayIndex),
                nameof(TrainingRow.DayOfWeek),
                nameof(TrainingRow.RollingAverage))
            .Append(mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(mlContext.Regression.Trainers.Sdca(
                labelColumnName: nameof(TrainingRow.Quantity),
                featureColumnName: "Features"));

        var model = pipeline.Fit(dataView);
        var engine = mlContext.Model.CreatePredictionEngine<TrainingRow, PredictionRow>(model);
        var last = timeline[^1];
        var predictions = new List<decimal>();

        for (var future = 1; future <= 7; future++)
        {
            var score = engine.Predict(new TrainingRow
            {
                DayIndex = last.DayIndex + future,
                DayOfWeek = ((last.DayOfWeek + future - 1) % 7) + 1,
                RollingAverage = recentAverage <= 0m ? last.RollingAverage : (float)recentAverage
            }).Score;

            predictions.Add(Math.Max(0m, Math.Round((decimal)score, 4, MidpointRounding.AwayFromZero)));
        }

        return predictions.Count == 0
            ? recentAverage
            : Math.Round(predictions.Average(), 4, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyList<DailyConsumptionSample> NormalizeTimeline(IReadOnlyCollection<ConsumoDiarioHistoricoCompra> consumos)
    {
        if (consumos.Count == 0)
        {
            return Array.Empty<DailyConsumptionSample>();
        }

        var firstDay = consumos.Min(current => current.Fecha);
        var lastDay = consumos.Max(current => current.Fecha);
        var map = consumos.ToDictionary(current => current.Fecha, current => current.Cantidad);
        var rollingWindow = new Queue<decimal>();
        var timeline = new List<DailyConsumptionSample>();

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

    private static string BuildJustification(EstudioMercadoTopProducto topProducto, decimal demandaMensualEsperada, decimal stockActual, decimal cantidadRecomendada)
    {
        if (cantidadRecomendada <= 0m)
        {
            return $"La demanda esperada del proximo mes es {demandaMensualEsperada:0.##} unidades y el stock actual de {stockActual:0.##} ya cubre el escenario con inventario de seguridad.";
        }

        return $"Se proyectan {demandaMensualEsperada:0.##} unidades para el proximo mes. Con margen estimado de {topProducto.MargenEstimado:C2} y stock actual de {stockActual:0.##}, se propone reponer {cantidadRecomendada:0.##} unidades incluyendo 15% de seguridad.";
    }

    private static string BuildStrategicAnalysis(
        IReadOnlyCollection<EstudioMercadoTopProducto> topProductos,
        IReadOnlyCollection<EstudioMercadoSugerenciaCompra> sugerencias,
        IReadOnlyCollection<EstudioMercadoCompra> estudiosHistoricos,
        int diasMesProximo)
    {
        if (topProductos.Count == 0)
        {
            return "No se detectaron ventas en el periodo consultado, por lo que el sistema no identifica productos estrella ni una propuesta de compra automatica para el proximo mes.";
        }

        var estrella = topProductos
            .OrderByDescending(current => current.CantidadVendida)
            .ThenByDescending(current => current.MargenEstimado)
            .First();

        var productosConReposicion = sugerencias
            .Where(current => current.CantidadRecomendada > 0m)
            .OrderByDescending(current => current.CantidadRecomendada)
            .ToArray();

        var historicos = ExtractHistoricos(estudiosHistoricos);
        var repetidos = topProductos.Count(current => historicos.Contains(current.ProductoId));
        var inversionSugerida = sugerencias.Sum(current => current.CostoEstimado);

        var bloques = new List<string>
        {
            $"El producto lider del periodo es {estrella.Nombre}, con {estrella.CantidadVendida:0.##} unidades vendidas y un margen estimado de {estrella.MargenEstimado:C2}. La proyeccion local prepara una cobertura de {diasMesProximo} dias para el siguiente ciclo comercial.",
            productosConReposicion.Length == 0
                ? "La cobertura agregada de inventario soporta la demanda esperada del proximo mes incluso aplicando un 15% de stock de seguridad sobre los articulos de alta rotacion."
                : $"Se sugiere invertir {inversionSugerida:C2} en reposicion selectiva. Los faltantes mas importantes aparecen en {string.Join(", ", productosConReposicion.Take(3).Select(current => current.Nombre))}."
        };

        if (estudiosHistoricos.Count > 0)
        {
            bloques.Add($"La memoria acumulada indica que {repetidos} de los productos estrella del mes ya venian apareciendo en estudios previos, una senal util para negociar abastecimiento recurrente y detectar estacionalidad temprana.");
        }
        else
        {
            bloques.Add("Este es el primer estudio guardado para esta empresa, asi que servira como base para construir aprendizaje de estacionalidad y repeticion de patrones de compra en consultas futuras.");
        }

        bloques.Add("La recomendacion operativa es priorizar compras en los productos con mayor rotacion y margen, evitando sobrestock en los articulos cuyo inventario actual ya cubre la demanda esperada.");

        return string.Join(Environment.NewLine + Environment.NewLine, bloques);
    }

    private static HashSet<Guid> ExtractHistoricos(IReadOnlyCollection<EstudioMercadoCompra> estudiosHistoricos)
    {
        var ids = new HashSet<Guid>();

        foreach (var estudio in estudiosHistoricos)
        {
            try
            {
                var top = JsonSerializer.Deserialize<IReadOnlyCollection<EstudioMercadoTopProducto>>(estudio.TopProductosVendidosJson);
                if (top is null)
                {
                    continue;
                }

                foreach (var producto in top)
                {
                    ids.Add(producto.ProductoId);
                }
            }
            catch (JsonException)
            {
            }
        }

        return ids;
    }

    private sealed class DailyConsumptionSample
    {
        public float DayIndex { get; init; }
        public float DayOfWeek { get; init; }
        public float Quantity { get; init; }
        public float RollingAverage { get; init; }
    }

    private sealed class TrainingRow
    {
        public float DayIndex { get; init; }
        public float DayOfWeek { get; init; }
        public float RollingAverage { get; init; }
        public float Quantity { get; init; }
    }

    private sealed class PredictionRow
    {
        public float Score { get; init; }
    }
}
