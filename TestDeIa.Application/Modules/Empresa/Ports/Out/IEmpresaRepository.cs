using TestDeIa.Domain.Modules.Empresa.Entities;

namespace TestDeIa.Application.Modules.Empresa.Ports.Out;

public interface IEmpresaRepository
{
    Task<EmpresaEmisora?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<EmpresaEmisora>> GetMineAsync(CancellationToken cancellationToken = default);
    Task<EmpresaEmisora?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmpresaEmisora> SaveAsync(EmpresaEmisora empresa, CancellationToken cancellationToken = default);
}
