using TestDeIa.Domain.Modules.Integraciones.Enums;

namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class EmpresaConfiguracionServiciosEntity
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string? WhatsAppApiToken { get; set; }

    public string? WhatsAppPhoneId { get; set; }

    public string? WhatsAppBusinessAccountId { get; set; }

    public string? PayPhoneToken { get; set; }

    public string? PayPhoneClientAppId { get; set; }

    public PasarelaPagoActiva PasarelaPagoActiva { get; set; } = PasarelaPagoActiva.Ninguna;

    public ModoPagosAmbiente ModopagosAmbiente { get; set; } = ModoPagosAmbiente.Pruebas;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;
}
