namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class FacturaSriEventoEntity
{
    public Guid Id { get; set; }

    public Guid FacturaId { get; set; }

    public FacturaEntity Factura { get; set; } = default!;

    public string Estado { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
