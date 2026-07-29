namespace TestDeIa.Shared.Reports.Facturacion;

public sealed class NotaCreditoRideTotalImpuestoDto
{
    public string CodigoIva { get; set; } = string.Empty;

    public decimal PorcentajeIva { get; set; }

    public decimal BaseImponible { get; set; }

    public decimal Valor { get; set; }
}
