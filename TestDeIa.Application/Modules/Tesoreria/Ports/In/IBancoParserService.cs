using TestDeIa.Domain.Modules.Tesoreria.Entities;

namespace TestDeIa.Application.Modules.Tesoreria.Ports.In;

public interface IBancoParserService
{
    Task<IReadOnlyCollection<ExtractoBancarioDetalle>> ParsearExtractoBancarioAsync(
        Stream fileStream,
        string extension,
        string bancoNombre,
        CancellationToken cancellationToken = default);
}
