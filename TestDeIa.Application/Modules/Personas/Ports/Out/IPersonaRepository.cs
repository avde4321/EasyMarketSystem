using TestDeIa.Domain.Modules.Personas.Entities;

namespace TestDeIa.Application.Modules.Personas.Ports.Out;

public interface IPersonaRepository
{
    Task<IReadOnlyCollection<Persona>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Persona?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Persona?> FindByIdentificacionAsync(string identificacion, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdentificacionAsync(string identificacion, Guid? excludedId = null, CancellationToken cancellationToken = default);

    Task<Persona> CreateAsync(Persona persona, CancellationToken cancellationToken = default);

    Task<Persona?> UpdateAsync(Persona persona, CancellationToken cancellationToken = default);
}
