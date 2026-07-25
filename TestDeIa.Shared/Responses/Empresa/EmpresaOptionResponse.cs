namespace TestDeIa.Shared.Responses.Empresa;

public sealed class EmpresaOptionResponse
{
    public Guid Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string AmbienteSri { get; set; } = "1";
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
