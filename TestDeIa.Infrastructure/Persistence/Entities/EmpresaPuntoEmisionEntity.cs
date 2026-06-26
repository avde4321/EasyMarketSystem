namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class EmpresaPuntoEmisionEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaEmisoraId { get; set; }
    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public string? DireccionEstablecimiento { get; set; }
    public bool IsDefault { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;
}
