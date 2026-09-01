using TestDeIa.Domain.Modules.Retenciones.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class ComprobanteRetencionEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? CompraId { get; set; }

    public Guid ProveedorId { get; set; }

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public string Secuencial { get; set; } = string.Empty;

    public string? ClaveAcceso { get; set; }

    public DateTimeOffset FechaEmision { get; set; }

    public AmbienteSriRetencion AmbienteSRI { get; set; } = AmbienteSriRetencion.Pruebas;

    public EstadoSriRetencion EstadoSRI { get; set; } = EstadoSriRetencion.Borrador;

    public string? NumeroAutorizacion { get; set; }

    public DateTimeOffset? FechaAutorizacion { get; set; }

    public string? MensajeErrorSRI { get; set; }

    public decimal TotalRetenido { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ComprobanteRetencionDetalleEntity> Detalles { get; set; } = [];
}
