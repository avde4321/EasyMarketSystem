namespace TestDeIa.Application.Modules.Sri.Models;

public sealed class DocumentoStorageReadResult
{
    public byte[] Content { get; init; } = [];

    public string HashSHA256 { get; init; } = string.Empty;

    public long TamanoBytes { get; init; }
}
