using TestDeIa.Shared.Responses.Retenciones;

namespace TestDeIa.Application.Modules.Retenciones.Ports.In;

public interface IRetencionSriTestService
{
    Task<RetencionSriTestResultResponse> ValidarXmlRetencionAsync(Guid retencionId, CancellationToken cancellationToken = default);

    Task<RetencionSriTestResultResponse> ValidarFirmaXadesBesAsync(string xmlGenerado, byte[] certificadoP12, string password, CancellationToken cancellationToken = default);

    Task<RetencionSriTestResultResponse> ProbarRecepcionSriPruebasAsync(byte[] xmlFirmado, CancellationToken cancellationToken = default);

    Task<RetencionSriTestResultResponse> ProbarAutorizacionSriPruebasAsync(string claveAcceso, CancellationToken cancellationToken = default);
}
