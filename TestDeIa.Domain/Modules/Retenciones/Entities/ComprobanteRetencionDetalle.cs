namespace TestDeIa.Domain.Modules.Retenciones.Entities;

public sealed record ComprobanteRetencionDetalle(
    Guid Id,
    Guid ComprobanteRetencionId,
    string CodigoImpuesto,
    string CodigoRetencionSRI,
    decimal BaseImponible,
    decimal PorcentajeRetencion,
    decimal ValorRetenido,
    string CodDocSustento,
    string NumDocSustento,
    DateTimeOffset FechaEmisionDocSustento);
