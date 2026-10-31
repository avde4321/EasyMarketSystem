namespace TestDeIa.Shared.Responses.Compras;

public sealed class FacturaProveedorAnalisisDetalleResponse
{
    public string CodigoPrincipal { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal TarifaIva { get; set; }

    public decimal Subtotal { get; set; }
}
