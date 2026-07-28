namespace TestDeIa.Shared.Responses.Financiero;

public sealed class ConsolidadoIvaMensualResponse
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public ConsolidadoIvaTarifaResponse Ventas { get; set; } = new();
    public ConsolidadoIvaTarifaResponse VentasBienes { get; set; } = new();
    public ConsolidadoIvaTarifaResponse VentasServicios { get; set; } = new();
    public ConsolidadoIvaTarifaResponse Compras { get; set; } = new();
    public ConsolidadoIvaServiciosOperativosResponse ServiciosOperativos { get; set; } = new();
    public string RazonamientoIA { get; set; } = string.Empty;
    public string ContextoPrevioUtilizado { get; set; } = string.Empty;
    public bool TieneAprendizajeAcumulado { get; set; }
    public decimal DebitoFiscal { get; set; }
    public decimal CreditoFiscal { get; set; }
    public decimal IvaNetoPagar { get; set; }
    public decimal CreditoTributario { get; set; }
    public decimal FactorProporcionalidad { get; set; }
}
