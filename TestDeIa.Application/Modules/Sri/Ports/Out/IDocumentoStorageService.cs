using TestDeIa.Application.Modules.Sri.Models;

namespace TestDeIa.Application.Modules.Sri.Ports.Out;

public interface IDocumentoStorageService
{
    Task<DocumentoStorageResult> SaveAsync(
        DocumentoStorageRequest request,
        CancellationToken cancellationToken = default);

    Task<DocumentoStorageReadResult> ReadAsync(
        string rutaStorage,
        bool esCifrado,
        string expectedHashSha256,
        CancellationToken cancellationToken = default);
}
