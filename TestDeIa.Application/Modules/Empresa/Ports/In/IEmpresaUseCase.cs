using TestDeIa.Shared.Requests.Empresa;
using TestDeIa.Shared.Responses.Empresa;

namespace TestDeIa.Application.Modules.Empresa.Ports.In;

public interface IEmpresaUseCase
{
    Task<EmpresaResponse?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<EmpresaOptionResponse>> GetMineAsync(CancellationToken cancellationToken = default);
    Task<EmpresaResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmpresaResponse> SaveAsync(Guid? id, EmpresaRequest request, CancellationToken cancellationToken = default);
}
