using TestDeIa.Application.Modules.Sri.Models;

namespace TestDeIa.Application.Modules.Sri.Ports.Out;

public interface IDocumentoStorageService
{
    Task<DocumentoStorageResult> SaveAsync(
        DocumentoStorageRequest request,
        CancellationToken cancellationToken = default);
}
