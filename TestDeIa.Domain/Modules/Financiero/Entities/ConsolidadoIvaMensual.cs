namespace TestDeIa.Domain.Modules.Financiero.Entities;

public sealed class ConsolidadoIvaMensual
{
    public int Mes { get; init; }
    public int Anio { get; init; }
    public ConsolidadoIvaTarifa Ventas { get; init; } = new();
    public ConsolidadoIvaTarifa VentasBienes { get; init; } = new();
    public ConsolidadoIvaTarifa VentasServicios { get; init; } = new();
    public ConsolidadoIvaTarifa Compras { get; init; } = new();
    public ConsolidadoIvaServiciosOperativos ServiciosOperativos { get; init; } = new();
    public string RazonamientoIA { get; init; } = string.Empty;
    public string ContextoPrevioUtilizado { get; init; } = string.Empty;
    public bool TieneAprendizajeAcumulado { get; init; }
    public decimal DebitoFiscal => Ventas.IvaTarifaDiferenteCero;
    public decimal CreditoFiscal => Compras.IvaTarifaDiferenteCero;
    public decimal IvaNetoPagar => Math.Max(0m, DebitoFiscal - CreditoFiscal);
    public decimal CreditoTributario => Math.Max(0m, CreditoFiscal - DebitoFiscal);
    public decimal FactorProporcionalidad => DebitoFiscal <= 0m
        ? 0m
        : Math.Round(CreditoFiscal / DebitoFiscal, 4, MidpointRounding.AwayFromZero);
}
