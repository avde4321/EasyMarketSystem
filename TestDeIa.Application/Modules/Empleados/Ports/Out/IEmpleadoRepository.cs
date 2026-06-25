using TestDeIa.Domain.Modules.Empleados.Entities;
using TestDeIa.Shared.Responses.Common;

namespace TestDeIa.Application.Modules.Empleados.Ports.Out;

public interface IEmpleadoRepository
{
    Task<IReadOnlyCollection<Empleado>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResultResponse<Empleado>> GetPagedAsync(string? term, int skip, int take, CancellationToken cancellationToken = default);

    Task<Empleado?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Empleado?> GetByPersonaIdAsync(Guid personaId, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<Empleado> CreateAsync(Empleado empleado, CancellationToken cancellationToken = default);

    Task<Empleado?> UpdateAsync(Empleado empleado, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
