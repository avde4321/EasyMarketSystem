using TestDeIa.Shared.Requests.Integraciones;

namespace TestDeIa.Application.Modules.Integraciones.Ports.In;

public interface IWhatsAppService
{
    Task<WhatsAppEnvioResponseDto> EnviarComprobanteWhatsAppAsync(
        EnviarWhatsAppDto dto,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<WhatsAppEnvioResponseDto> EnviarRecordatorioPagoAsync(
        EnviarRecordatorioPagoDto dto,
        Guid empresaId,
        CancellationToken cancellationToken = default);
}
