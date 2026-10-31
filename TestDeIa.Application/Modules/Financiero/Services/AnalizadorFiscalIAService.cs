using System.Text.Json;
using TestDeIa.Domain.Modules.Financiero.Entities;

namespace TestDeIa.Application.Modules.Financiero.Services;

public sealed class AnalizadorFiscalIAService : IAnalizadorFiscalIAService
{
    public AnalisisFiscalIAResult Analizar(
        ConsolidadoIvaMensual consolidadoActual,
        IReadOnlyCollection<MemoriaAnalisisFiscal> memoriasHistoricas)
    {
        var historicos = memoriasHistoricas
            .Select(TryParseResumen)
            .Where(current => current is not null)
            .Cast<FiscalNumericSummary>()
            .ToArray();

        var contextoPrevioUtilizado = memoriasHistoricas.Count == 0
            ? "Sin memoria historica previa para comparar."
            : $"Se consideraron {memoriasHistoricas.Count} periodos previos: {string.Join(", ", memoriasHistoricas.Select(FormatPeriodo))}.";

        var razonamientos = new List<string>
        {
            BuildPanoramaActual(consolidadoActual)
        };

        if (historicos.Length == 0)
        {
            razonamientos.Add("Aun no existe suficiente memoria acumulada para detectar una tendencia sostenida, asi que este analisis parte principalmente del comportamiento del periodo consultado.");
        }
        else
        {
            razonamientos.Add(BuildComparativaHistorica(consolidadoActual, historicos));
        }

        razonamientos.Add(BuildFiscalHint(consolidadoActual));

        return new AnalisisFiscalIAResult
        {
            RazonamientoIA = string.Join(Environment.NewLine + Environment.NewLine, razonamientos.Where(current => !string.IsNullOrWhiteSpace(current))),
            ContextoPrevioUtilizado = contextoPrevioUtilizado,
            TieneAprendizajeAcumulado = historicos.Length > 0
        };
    }

    private static string BuildPanoramaActual(ConsolidadoIvaMensual consolidadoActual)
    {
        var ventasGravadas = consolidadoActual.Ventas.BaseTarifaDiferenteCero;
        var comprasGravadas = consolidadoActual.Compras.BaseTarifaDiferenteCero;
        var ratioCredito = consolidadoActual.DebitoFiscal <= 0m
            ? 0m
            : Math.Round(consolidadoActual.CreditoFiscal / consolidadoActual.DebitoFiscal, 4, MidpointRounding.AwayFromZero);

        return $"En {FormatPeriodo(consolidadoActual.Mes, consolidadoActual.Anio)} se registran ventas gravadas por {ventasGravadas:C2}, compras gravadas por {comprasGravadas:C2}, debito fiscal de {consolidadoActual.DebitoFiscal:C2} y credito fiscal de {consolidadoActual.CreditoFiscal:C2}. El ratio de aprovechamiento del credito fiscal frente al debito es {ratioCredito:0.0000}.";
    }

    private static string BuildComparativaHistorica(ConsolidadoIvaMensual consolidadoActual, IReadOnlyCollection<FiscalNumericSummary> historicos)
    {
        var promedioDebito = historicos.Average(current => current.DebitoFiscal);
        var promedioCredito = historicos.Average(current => current.CreditoFiscal);
        var promedioVentasGravadas = historicos.Average(current => current.VentasBaseTarifaDiferenteCero);
        var promedioComprasGravadas = historicos.Average(current => current.ComprasBaseTarifaDiferenteCero);

        var tendenciaDebito = DescribeTrend(consolidadoActual.DebitoFiscal, promedioDebito, "debito fiscal");
        var tendenciaCredito = DescribeTrend(consolidadoActual.CreditoFiscal, promedioCredito, "credito fiscal");
        var tendenciaVentas = DescribeTrend(consolidadoActual.Ventas.BaseTarifaDiferenteCero, promedioVentasGravadas, "ventas gravadas");
        var tendenciaCompras = DescribeTrend(consolidadoActual.Compras.BaseTarifaDiferenteCero, promedioComprasGravadas, "compras gravadas");

        return $"Comparado con la memoria de los ultimos {historicos.Count} periodos, el {tendenciaDebito}; el {tendenciaCredito}; las {tendenciaVentas}; y las {tendenciaCompras}.";
    }

    private static string BuildFiscalHint(ConsolidadoIvaMensual consolidadoActual)
    {
        if (consolidadoActual.CreditoTributario > 0m)
        {
            return $"El periodo cierra con credito tributario de {consolidadoActual.CreditoTributario:C2}. Conviene revisar si el peso de compras gravadas o adquisiciones de inventario fue excepcional para sostener el soporte documental del arrastre fiscal.";
        }

        if (consolidadoActual.IvaNetoPagar > 0m)
        {
            return $"El IVA neto estimado a pagar es {consolidadoActual.IvaNetoPagar:C2}. Te conviene validar que las compras con derecho a credito y las retenciones soportadas del mes ya esten registradas antes del cierre.";
        }

        return "El balance fiscal del mes queda compensado. Aun asi, vale revisar que las tarifas 0%, 5%, 8% y 15% esten clasificadas correctamente para evitar desfaces en el Formulario 104.";
    }

    private static string DescribeTrend(decimal actual, decimal promedio, string concepto)
    {
        if (promedio <= 0m && actual <= 0m)
        {
            return $"{concepto} se mantiene sin movimiento relevante";
        }

        if (promedio <= 0m)
        {
            return $"{concepto} aparece por primera vez con un nivel material";
        }

        var variacion = Math.Round(((actual - promedio) / promedio) * 100m, 2, MidpointRounding.AwayFromZero);

        if (Math.Abs(variacion) < 5m)
        {
            return $"{concepto} se mantiene estable frente al promedio historico";
        }

        return variacion > 0m
            ? $"{concepto} crece {variacion:0.##}% sobre el promedio historico"
            : $"{concepto} cae {Math.Abs(variacion):0.##}% frente al promedio historico";
    }

    private static FiscalNumericSummary? TryParseResumen(MemoriaAnalisisFiscal memoria)
    {
        try
        {
            return JsonSerializer.Deserialize<FiscalNumericSummary>(memoria.ResumenNumericoJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string FormatPeriodo(MemoriaAnalisisFiscal memoria)
    {
        return FormatPeriodo(memoria.Mes, memoria.Anio);
    }

    private static string FormatPeriodo(int mes, int anio)
    {
        return $"{mes:00}/{anio}";
    }

    private sealed class FiscalNumericSummary
    {
        public decimal VentasBaseTarifaDiferenteCero { get; set; }
        public decimal ComprasBaseTarifaDiferenteCero { get; set; }
        public decimal DebitoFiscal { get; set; }
        public decimal CreditoFiscal { get; set; }
        public decimal IvaNetoPagar { get; set; }
        public decimal CreditoTributario { get; set; }
    }
}
