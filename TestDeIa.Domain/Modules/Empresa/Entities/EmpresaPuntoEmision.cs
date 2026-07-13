namespace TestDeIa.Domain.Modules.Empresa.Entities;

public sealed class EmpresaPuntoEmision
{
    public EmpresaPuntoEmision(
        Guid id,
        Guid empresaId,
        string establecimiento,
        string puntoEmision,
        string? direccionEstablecimiento,
        bool isDefault,
        Guid? bodegaId,
        string? bodegaNombre)
    {
        Id = id;
        EmpresaId = empresaId;
        Establecimiento = establecimiento;
        PuntoEmision = puntoEmision;
        DireccionEstablecimiento = direccionEstablecimiento;
        IsDefault = isDefault;
        BodegaId = bodegaId;
        BodegaNombre = bodegaNombre;
    }

    public Guid Id { get; }
    public Guid EmpresaId { get; }
    public string Establecimiento { get; }
    public string PuntoEmision { get; }
    public string? DireccionEstablecimiento { get; }
    public bool IsDefault { get; }
    public Guid? BodegaId { get; }
    public string? BodegaNombre { get; }
}
