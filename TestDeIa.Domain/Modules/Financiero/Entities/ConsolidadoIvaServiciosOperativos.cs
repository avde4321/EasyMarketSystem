namespace TestDeIa.Domain.Modules.Financiero.Entities;

public sealed class ConsolidadoIvaServiciosOperativos
{
    public string PuntoEmision { get; init; } = string.Empty;
    public string Cajero { get; init; } = string.Empty;
    public decimal TotalFacturadoServicios { get; init; }
    public int FacturasProcesadas { get; init; }
}
