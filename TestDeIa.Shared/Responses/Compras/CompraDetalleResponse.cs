namespace TestDeIa.Shared.Responses.Compras;

public sealed class CompraDetalleResponse
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public string CodigoIva { get; set; } = string.Empty;
    public decimal PorcentajeIva { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal CostoTotalSinImpuesto { get; set; }
    public decimal TotalImpuesto { get; set; }
    public decimal TotalLinea { get; set; }
}
