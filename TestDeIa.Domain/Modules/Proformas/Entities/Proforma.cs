using TestDeIa.Domain.Modules.Proformas.Enums;

namespace TestDeIa.Domain.Modules.Proformas.Entities;

public sealed record Proforma(
    Guid Id,
    Guid EmpresaId,
    Guid ClienteId,
    Guid UsuarioId,
    Guid BodegaId,
    string Secuencial,
    DateTimeOffset FechaEmision,
    DateTimeOffset? FechaVencimiento,
    EstadoProforma Estado,
    decimal SubtotalSinImpuestos,
    decimal SubtotalIVA,
    decimal DescuentoTotal,
    decimal Total,
    string? Observacion,
    Guid? FacturaId,
    IReadOnlyCollection<ProformaDetalle> Detalles);
