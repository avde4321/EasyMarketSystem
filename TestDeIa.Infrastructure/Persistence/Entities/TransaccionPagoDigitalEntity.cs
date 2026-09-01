using TestDeIa.Domain.Modules.Integraciones.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class TransaccionPagoDigitalEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? FacturaId { get; set; }

    public Guid ClienteId { get; set; }

    public decimal Monto { get; set; }

    public string Pasarela { get; set; } = string.Empty;

    public string TransactionIdPasarela { get; set; } = string.Empty;

    public EstadoPagoDigital EstadoPago { get; set; } = EstadoPagoDigital.Pendiente;

    public string? LinkPagoUrl { get; set; }

    public string? QrCodeBase64 { get; set; }

    public DateTimeOffset FechaCreacion { get; set; }

    public DateTimeOffset? FechaAprobacion { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;

    public FacturaEntity? Factura { get; set; }

    public ClienteEntity Cliente { get; set; } = default!;
}
