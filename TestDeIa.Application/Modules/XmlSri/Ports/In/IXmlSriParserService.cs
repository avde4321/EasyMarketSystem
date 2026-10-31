using TestDeIa.Shared.Requests.XmlSri;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Compras;
using TestDeIa.Shared.Responses.XmlSri;

namespace TestDeIa.Application.Modules.XmlSri.Ports.In;

public interface IXmlSriParserService
{
    XmlSriParserResultDto ParsearXmlFacturaSri(string xmlContent);

    Task<XmlSriCargaMasivaResponse> ProcesarArchivosXmlMasivosAsync(
        IReadOnlyCollection<string> xmlsContent,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<PagedResultResponse<FacturaCompraXmlLogResponse>> GetPendientesAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<XmlSriParserResultDto> GetParsedAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<string> GetXmlOriginalAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CompraResponse> ConvertirXmlACompraAsync(
        ConvertirXmlACompraRequest request,
        CancellationToken cancellationToken = default);
}
