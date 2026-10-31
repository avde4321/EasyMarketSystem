namespace TestDeIa.Shared.Responses.Security;

public sealed class SecurityPointEmissionResponse
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid BodegaId { get; set; }

    public string Establecimiento { get; set; } = string.Empty;

    public string PuntoEmision { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? DireccionEstablecimiento { get; set; }

    public bool IsDefault { get; set; }
}
