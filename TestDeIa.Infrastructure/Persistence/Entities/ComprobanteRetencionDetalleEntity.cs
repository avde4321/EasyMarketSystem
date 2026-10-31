namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ComprobanteRetencionDetalleEntity
{
    public Guid Id { get; set; }

    public Guid ComprobanteRetencionId { get; set; }

    public ComprobanteRetencionEntity ComprobanteRetencion { get; set; } = default!;

    public string CodigoImpuesto { get; set; } = string.Empty;

    public string CodigoRetencionSRI { get; set; } = string.Empty;

    public decimal BaseImponible { get; set; }

    public decimal PorcentajeRetencion { get; set; }

    public decimal ValorRetenido { get; set; }

    public string CodDocSustento { get; set; } = string.Empty;

    public string NumDocSustento { get; set; } = string.Empty;

    public DateTimeOffset FechaEmisionDocSustento { get; set; }
}
