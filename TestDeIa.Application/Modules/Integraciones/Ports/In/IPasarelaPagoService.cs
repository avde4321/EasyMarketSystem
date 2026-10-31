using TestDeIa.Shared.Requests.Integraciones;
using TestDeIa.Shared.Responses.Integraciones;

namespace TestDeIa.Application.Modules.Integraciones.Ports.In;

public interface IPasarelaPagoService
{
    Task<RespuestaPagoDigitalDto> GenerarEnlaceCobroAsync(
        GenerarLinkPagoDto dto,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<RespuestaPagoDigitalDto> ProcesarWebhookPagoAsync(
        PayPhoneWebhookDto dto,
        string? authorizationToken,
        CancellationToken cancellationToken = default);

    Task<RespuestaPagoDigitalDto> ConsultarEstadoAsync(
        string transactionId,
        Guid empresaId,
        CancellationToken cancellationToken = default);
}
