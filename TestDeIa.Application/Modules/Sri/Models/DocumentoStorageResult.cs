namespace TestDeIa.Application.Modules.Sri.Models;

public sealed class DocumentoStorageResult
{
    public string RutaStorage { get; init; } = string.Empty;

    public string HashSHA256 { get; init; } = string.Empty;

    public long TamanoBytes { get; init; }

    public bool EsCifrado { get; init; }

    public string? AlgoritmoCifrado { get; init; }
}
