namespace TestDeIa.Shared.Responses.Facturacion;

public sealed class PosPuntoEmisionResponse
{
    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public string? DireccionEstablecimiento { get; set; }
    public bool IsDefault { get; set; }
    public string DisplayName => $"{Establecimiento}-{PuntoEmision}";
}
