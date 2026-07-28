namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class UserEmpresaAcceso
{
    public UserEmpresaAcceso(
        Guid empresaId,
        string razonSocial,
        string? nombreComercial,
        string ruc,
        string ambienteSri,
        bool isActive,
        bool isDefault)
    {
        EmpresaId = empresaId;
        RazonSocial = razonSocial;
        NombreComercial = nombreComercial;
        Ruc = ruc;
        AmbienteSri = ambienteSri;
        IsActive = isActive;
        IsDefault = isDefault;
    }

    public Guid EmpresaId { get; }
    public string RazonSocial { get; }
    public string? NombreComercial { get; }
    public string Ruc { get; }
    public string AmbienteSri { get; }
    public bool IsActive { get; }
    public bool IsDefault { get; }
}
