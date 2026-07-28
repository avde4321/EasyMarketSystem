using TestDeIa.Domain.Modules.ActivosFijos.Entities;
using TestDeIa.Domain.Modules.ActivosFijos.Enums;

namespace TestDeIa.Application.Modules.ActivosFijos.Ports.Out;

public interface IActivoFijoRepository
{
    Task<IReadOnlyCollection<ActivoFijo>> GetAsync(
        CategoriaSriActivoFijo? categoria,
        string? custodio,
        EstadoActivoFijo? estado,
        CancellationToken cancellationToken = default);

    Task<ActivoFijo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActivoFijo> CreateAsync(ActivoFijo activoFijo, CancellationToken cancellationToken = default);

    Task<ActivoFijo?> UpdateAsync(ActivoFijo activoFijo, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCompraDetalleIdAsync(Guid compraDetalleId, CancellationToken cancellationToken = default);

    Task<string> ReserveNextCodigoAsync(DateTime fechaAdquisicion, CancellationToken cancellationToken = default);
}
