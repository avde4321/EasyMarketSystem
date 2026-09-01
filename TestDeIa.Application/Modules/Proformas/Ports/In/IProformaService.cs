using TestDeIa.Shared.Requests.Proformas;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Facturacion;
using TestDeIa.Shared.Responses.Proformas;

namespace TestDeIa.Application.Modules.Proformas.Ports.In;

public interface IProformaService
{
    Task<PagedResultResponse<ProformaResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<ProformaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProformaResponse> CreateAsync(ProformaRequest request, CancellationToken cancellationToken = default);

    Task<ProformaResponse?> UpdateAsync(Guid id, ProformaRequest request, CancellationToken cancellationToken = default);

    Task<bool> AnularAsync(Guid id, CancellationToken cancellationToken = default);

    Task<FacturaEmissionResponse> ConvertirProformaAFacturaAsync(ConvertirProformaAFacturaDto request, CancellationToken cancellationToken = default);
}
