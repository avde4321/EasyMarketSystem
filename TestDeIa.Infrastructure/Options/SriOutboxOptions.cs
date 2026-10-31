namespace TestDeIa.Infrastructure.Options;

public sealed class SriOutboxOptions
{
    public const string SectionName = "SriOutbox";

    public bool Enabled { get; set; }

    public int DelaySeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 20;

    public int MaxIntentos { get; set; } = 3;
}
