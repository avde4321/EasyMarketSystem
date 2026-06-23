using TestDeIa.Domain.Modules.Clientes.Entities;

namespace TestDeIa.Application.Modules.Clientes.Ports.Out;

public interface IClienteRepository
{
    Task<IReadOnlyCollection<Cliente>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Cliente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdentificacionAsync(string identificacion, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsPersonaByIdentificacionAsync(string identificacion, Guid? excludedPersonaId = null, CancellationToken cancellationToken = default);

    Task<Cliente> CreateAsync(Cliente cliente, CancellationToken cancellationToken = default);

    Task<Cliente?> UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
