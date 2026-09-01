namespace TestDeIa.Infrastructure.Options;

public sealed class DocumentoStorageOptions
{
    public const string SectionName = "DocumentoStorage";

    public string RootPath { get; set; } = "storage";
}
