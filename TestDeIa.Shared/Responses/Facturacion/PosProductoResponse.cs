namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class PosProductoResponse
{
    public Guid ProductoId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string CodigoIva { get; set; } = string.Empty;

    public decimal PorcentajeIva { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal StockActual { get; set; }
}
