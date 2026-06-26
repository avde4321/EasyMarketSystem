namespace TestDeIa.Shared.Responses.Empresa;

public sealed class EmpresaPuntoEmisionResponse
{
    public Guid Id { get; set; }
    public string? DireccionEstablecimiento { get; set; }
    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
