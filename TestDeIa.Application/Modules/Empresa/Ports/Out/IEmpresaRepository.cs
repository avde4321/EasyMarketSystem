using TestDeIa.Domain.Modules.Empresa.Entities;

namespace TestDeIa.Application.Modules.Empresa.Ports.Out;

public interface IEmpresaRepository
{
    Task<EmpresaEmisora?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<EmpresaEmisora> UpsertAsync(EmpresaEmisora empresa, CancellationToken cancellationToken = default);
}
