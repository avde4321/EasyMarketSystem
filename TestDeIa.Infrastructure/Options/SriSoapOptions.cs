namespace TestDeIa.Infrastructure.Options;

public sealed class SriSoapOptions
{
    public const string SectionName = "SriOffline";

    public int TimeoutSeconds { get; set; } = 30;

    public SriSoapEnvironmentOptions Pruebas { get; set; } = new();

    public SriSoapEnvironmentOptions Produccion { get; set; } = new();
}

public sealed class SriSoapEnvironmentOptions
{
    public string RecepcionUrl { get; set; } = string.Empty;

    public string AutorizacionUrl { get; set; } = string.Empty;
}
