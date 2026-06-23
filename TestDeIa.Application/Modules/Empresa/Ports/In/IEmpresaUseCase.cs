using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Application.Modules.Empresa.Ports.In;

public interface IEmpresaUseCase
{
    Task<EmpresaResponse?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<EmpresaResponse> UpsertAsync(EmpresaRequest request, CancellationToken cancellationToken = default);
}
