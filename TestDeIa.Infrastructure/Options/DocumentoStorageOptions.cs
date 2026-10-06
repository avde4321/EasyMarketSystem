namespace TestDeIa.Infrastructure.Options;

public sealed class DocumentoStorageOptions
{
    public const string SectionName = "DocumentoStorage";

    public string RootPath { get; set; } = "storage";

    public string? EncryptionKeyBase64 { get; set; }

    public long DefaultMaxBytes { get; set; } = 10 * 1024 * 1024;

    public long CertificateMaxBytes { get; set; } = 5 * 1024 * 1024;
}
