namespace TestDeIa.Shared.Responses.Financiero;

public sealed class ConsolidadoIvaServiciosOperativosResponse
{
    public string PuntoEmision { get; set; } = string.Empty;
    public string Cajero { get; set; } = string.Empty;
    public decimal TotalFacturadoServicios { get; set; }
    public int FacturasProcesadas { get; set; }
}
