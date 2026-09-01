namespace TestDeIa.Shared.Responses.Proformas;

public sealed class ProformaDetalleResponse
{
    public Guid Id { get; set; }

    public Guid ProductoId { get; set; }

    public string ProductoCodigo { get; set; } = string.Empty;

    public string ProductoNombre { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal TarifaIVA { get; set; }

    public decimal ValorIVA { get; set; }

    public decimal Subtotal { get; set; }
}
