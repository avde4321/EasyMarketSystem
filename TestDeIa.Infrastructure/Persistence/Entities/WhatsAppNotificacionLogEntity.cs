using TestDeIa.Domain.Modules.Integraciones.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class WhatsAppNotificacionLogEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string NumeroDestino { get; set; } = string.Empty;

    public string TipoDocumento { get; set; } = string.Empty;

    public Guid DocumentoId { get; set; }

    public EstadoEnvioWhatsApp EstadoEnvio { get; set; } = EstadoEnvioWhatsApp.Pendiente;

    public string? MensajeError { get; set; }

    public DateTimeOffset FechaEnvio { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;
}
