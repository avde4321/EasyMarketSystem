using TestDeIa.Domain.Modules.Proformas.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ProformaEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid ClienteId { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid BodegaId { get; set; }

    public string Secuencial { get; set; } = string.Empty;

    public DateTimeOffset FechaEmision { get; set; }

    public DateTimeOffset? FechaVencimiento { get; set; }

    public EstadoProforma Estado { get; set; } = EstadoProforma.Borrador;

    public decimal SubtotalSinImpuestos { get; set; }

    public decimal SubtotalIVA { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal Total { get; set; }

    public string? Observacion { get; set; }

    public Guid? FacturaId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ProformaDetalleEntity> Detalles { get; set; } = [];
}
