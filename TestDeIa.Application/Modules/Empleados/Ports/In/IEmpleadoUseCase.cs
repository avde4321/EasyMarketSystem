using TestDeIa.Shared.Requests.Empleados;
using TestDeIa.Shared.Responses.Common;
using TestDeIa.Shared.Responses.Empleados;

namespace TestDeIa.Application.Modules.Empleados.Ports.In;

public interface IEmpleadoUseCase
{
    Task<IReadOnlyCollection<EmpleadoResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<EmpleadoResponse>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<EmpleadoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmpleadoResponse> CreateAsync(EmpleadoRequest request, CancellationToken cancellationToken = default);

    Task<EmpleadoResponse?> UpdateAsync(Guid id, EmpleadoRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
