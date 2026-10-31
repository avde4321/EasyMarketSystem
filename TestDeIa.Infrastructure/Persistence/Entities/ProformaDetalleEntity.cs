namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ProformaDetalleEntity
{
    public Guid Id { get; set; }

    public Guid ProformaId { get; set; }

    public ProformaEntity Proforma { get; set; } = default!;

    public Guid ProductoId { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal TarifaIVA { get; set; }

    public decimal ValorIVA { get; set; }

    public decimal Subtotal { get; set; }
}
