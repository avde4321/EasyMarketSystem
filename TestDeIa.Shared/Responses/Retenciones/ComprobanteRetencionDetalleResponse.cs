namespace TestDeIa.Shared.Responses.Retenciones;

public sealed class ComprobanteRetencionDetalleResponse
{
    public Guid Id { get; set; }

    public string CodigoImpuesto { get; set; } = string.Empty;

    public string CodigoRetencionSRI { get; set; } = string.Empty;

    public decimal BaseImponible { get; set; }

    public decimal PorcentajeRetencion { get; set; }

    public decimal ValorRetenido { get; set; }

    public string CodDocSustento { get; set; } = string.Empty;

    public string NumDocSustento { get; set; } = string.Empty;

    public DateTime FechaEmisionDocSustento { get; set; }
}
