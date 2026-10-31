using System.Globalization;
using TestDeIa.Domain.Modules.Facturacion.Entities;

namespace TestDeIa.Infrastructure.Adapters.Out.Facturacion;

internal static class SriTaxCatalog
{
    private const string CodigoImpuestoIva = "2";

    private static readonly IReadOnlyDictionary<decimal, string> DefaultCodesByRate = new Dictionary<decimal, string>
    {
        [0m] = "0",
        [5m] = "5",
        [8m] = "8",
        [15m] = "4"
    };

    private static readonly IReadOnlyDictionary<string, string> AliasCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["IVA_0"] = "0",
        ["IVA_5"] = "5",
        ["IVA_8"] = "8",
        ["IVA_15"] = "4",
        ["TARIFA_0"] = "0",
        ["TARIFA_5"] = "5",
        ["TARIFA_8"] = "8",
        ["TARIFA_15"] = "4"
    };

    internal static IReadOnlyCollection<TotalImpuestoDescriptor> BuildHeaderTotals(Factura factura)
    {
        var totals = new List<TotalImpuestoDescriptor>();
        AppendTotal(totals, factura, 0m, factura.SubtotalIva0);
        AppendTotal(totals, factura, 5m, factura.SubtotalIva5);
        AppendTotal(totals, factura, 8m, factura.SubtotalIva8);
        AppendTotal(totals, factura, 15m, factura.SubtotalIva15);
        return totals;
    }

    internal static SriTaxDescriptor ResolveDetalleTax(FacturaDetalle detalle)
    {
        var codigoPorcentaje = ResolveCodigoPorcentaje(detalle.CodigoIva, detalle.PorcentajeIva);
        return new SriTaxDescriptor(
            CodigoImpuestoIva,
            codigoPorcentaje,
            detalle.PorcentajeIva.ToString("0.##", CultureInfo.InvariantCulture));
    }

    internal static string ResolveCodigoPorcentaje(string? codigoIva, decimal porcentajeIva)
    {
        var normalized = codigoIva?.Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            if (normalized.All(char.IsDigit))
            {
                return normalized;
            }

            if (AliasCodes.TryGetValue(normalized, out var aliasCode))
            {
                return aliasCode;
            }
        }

        if (DefaultCodesByRate.TryGetValue(porcentajeIva, out var fallbackCode))
        {
            return fallbackCode;
        }

        throw new InvalidOperationException($"No existe un codigoPorcentaje SRI soportado para la tarifa IVA {porcentajeIva:0.##}%.");
    }

    private static void AppendTotal(ICollection<TotalImpuestoDescriptor> totals, Factura factura, decimal porcentajeIva, decimal subtotalBase)
    {
        if (subtotalBase <= 0)
        {
            return;
        }

        var detalles = factura.Detalles
            .Where(detalle => detalle.PorcentajeIva == porcentajeIva)
            .ToArray();

        if (detalles.Length == 0)
        {
            throw new InvalidOperationException($"La factura contiene subtotal {porcentajeIva:0.##}% en cabecera, pero no existen detalles con esa tarifa.");
        }

        var codigoPorcentaje = ResolveCodigoPorcentaje(detalles[0].CodigoIva, porcentajeIva);
        var valor = detalles.Sum(detalle => detalle.IvaValor);

        totals.Add(new TotalImpuestoDescriptor(
            CodigoImpuestoIva,
            codigoPorcentaje,
            subtotalBase,
            valor,
            porcentajeIva.ToString("0.##", CultureInfo.InvariantCulture)));
    }

    internal sealed record SriTaxDescriptor(string CodigoImpuesto, string CodigoPorcentaje, string TarifaTexto);

    internal sealed record TotalImpuestoDescriptor(
        string CodigoImpuesto,
        string CodigoPorcentaje,
        decimal BaseImponible,
        decimal Valor,
        string TarifaTexto);
}
