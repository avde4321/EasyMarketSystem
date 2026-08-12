using TestDeIa.Domain.Modules.Empresa.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Application.Modules.Empresa.Ports.Out;

public interface ICertificadoDigitalEmpresaRepository
{
    Task<PagedResultResponse<CertificadoDigitalEmpresa>> GetPagedAsync(
        string? term,
        Guid? empresaId,
        bool soloAlertas,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<CertificadoDigitalEmpresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CertificadoDigitalEmpresa>> GetAlertasAsync(CancellationToken cancellationToken = default);

    Task<CertificadoDigitalEmpresa> SaveAsync(CertificadoDigitalEmpresa certificado, bool activarComoPrincipal, CancellationToken cancellationToken = default);

    Task<CertificadoDigitalEmpresa> ActivarAsync(Guid id, CancellationToken cancellationToken = default);

    Task DesactivarAsync(Guid id, CancellationToken cancellationToken = default);
}
