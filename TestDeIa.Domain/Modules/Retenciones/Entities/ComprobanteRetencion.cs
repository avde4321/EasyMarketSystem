using TestDeIa.Domain.Modules.Retenciones.Enums;

namespace TestDeIa.Domain.Modules.Retenciones.Entities;

public sealed record ComprobanteRetencion(
    Guid Id,
    Guid EmpresaId,
    Guid? CompraId,
    Guid ProveedorId,
    string Establecimiento,
    string PuntoEmision,
    string Secuencial,
    string? ClaveAcceso,
    DateTimeOffset FechaEmision,
    AmbienteSriRetencion AmbienteSRI,
    EstadoSriRetencion EstadoSRI,
    string? NumeroAutorizacion,
    DateTimeOffset? FechaAutorizacion,
    string? MensajeErrorSRI,
    decimal TotalRetenido,
    IReadOnlyCollection<ComprobanteRetencionDetalle> Detalles);
