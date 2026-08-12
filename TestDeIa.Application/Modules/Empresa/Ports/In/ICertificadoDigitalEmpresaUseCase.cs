using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Application.Modules.Empresa.Ports.In;

public interface ICertificadoDigitalEmpresaUseCase
{
    Task<PagedResultResponse<CertificadoDigitalEmpresaResponse>> GetPagedAsync(
        string? term,
        Guid? empresaId,
        bool soloAlertas,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<CertificadoDigitalEmpresaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CertificadoDigitalEmpresaResponse>> GetAlertasAsync(CancellationToken cancellationToken = default);

    Task<CertificadoDigitalEmpresaResponse> CreateAsync(CertificadoDigitalEmpresaRequest request, CancellationToken cancellationToken = default);

    Task<CertificadoDigitalEmpresaResponse> ActivarAsync(Guid id, CancellationToken cancellationToken = default);

    Task DesactivarAsync(Guid id, CancellationToken cancellationToken = default);
}
