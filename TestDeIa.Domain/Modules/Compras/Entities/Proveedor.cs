namespace TestDeIa.Domain.Modules.Compras.Entities;

public sealed class Proveedor
{
    public Proveedor(
        Guid id,
        Guid personaId,
        Guid empresaId,
        string tipoIdentificacion,
        string identificacion,
        string razonSocialONombresCompletos,
        string? nombreComercial,
        string direccionPrincipal,
        string? correoElectronicoPrincipal,
        string? telefonoCelular,
        string codigoRetencionIvaDefault,
        string codigoRetencionRentaDefault,
        bool permiteCredito,
        int diasCredito,
        EstadoProveedor estadoProveedor,
        IReadOnlyCollection<string> rolesPersona,
        bool isActive,
        DateTimeOffset createdAt,
        Guid usuarioCreacionId,
        DateTimeOffset? updatedAt,
        Guid? usuarioModificacionId)
    {
        Id = id;
        PersonaId = personaId;
        EmpresaId = empresaId;
        TipoIdentificacion = tipoIdentificacion;
        Identificacion = identificacion;
        RazonSocialONombresCompletos = razonSocialONombresCompletos;
        NombreComercial = nombreComercial;
        DireccionPrincipal = direccionPrincipal;
        CorreoElectronicoPrincipal = correoElectronicoPrincipal;
        TelefonoCelular = telefonoCelular;
        CodigoRetencionIvaDefault = codigoRetencionIvaDefault;
        CodigoRetencionRentaDefault = codigoRetencionRentaDefault;
        PermiteCredito = permiteCredito;
        DiasCredito = diasCredito;
        EstadoProveedor = estadoProveedor;
        RolesPersona = rolesPersona;
        IsActive = isActive;
        CreatedAt = createdAt;
        UsuarioCreacionId = usuarioCreacionId;
        UpdatedAt = updatedAt;
        UsuarioModificacionId = usuarioModificacionId;
    }

    public Guid Id { get; }
    public Guid PersonaId { get; }
    public Guid EmpresaId { get; }
    public string TipoIdentificacion { get; }
    public string Identificacion { get; }
    public string RazonSocialONombresCompletos { get; }
    public string? NombreComercial { get; }
    public string NombreCompleto => RazonSocialONombresCompletos;
    public string DireccionPrincipal { get; }
    public string? CorreoElectronicoPrincipal { get; }
    public string? TelefonoCelular { get; }
    public string CodigoRetencionIvaDefault { get; }
    public string CodigoRetencionRentaDefault { get; }
    public bool PermiteCredito { get; }
    public int DiasCredito { get; }
    public EstadoProveedor EstadoProveedor { get; }
    public IReadOnlyCollection<string> RolesPersona { get; }
    public bool IsActive { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid UsuarioCreacionId { get; }
    public DateTimeOffset? UpdatedAt { get; }
    public Guid? UsuarioModificacionId { get; }
}