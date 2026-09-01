namespace TestDeIa.Application.Modules.Sri.Models;

public sealed class DocumentoStorageRequest
{
    public Guid EmpresaId { get; init; }

    public string Modulo { get; init; } = string.Empty;

    public string EntidadTipo { get; init; } = string.Empty;

    public Guid EntidadId { get; init; }

    public string TipoAdjunto { get; init; } = string.Empty;

    public string NombreArchivo { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public byte[] Content { get; init; } = [];

    public Guid? CreadoPorUsuarioId { get; init; }

    public string Origen { get; init; } = "Automatico";
}
