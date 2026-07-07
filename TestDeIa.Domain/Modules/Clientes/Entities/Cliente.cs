namespace TestDeIa.Domain.Modules.Clientes.Entities;

public sealed class Cliente
{
    public Cliente(
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
        DateOnly? fechaNacimiento,
        string? genero,
        string? correoFacturacionElectronica,
        string tipoCliente,
        bool obligadoContabilidad,
        bool esContribuyenteEspecial,
        bool permiteCredito,
        decimal limiteCredito,
        int diasCreditoMaximo,
        string estadoCredito,
        IReadOnlyCollection<string> rolesPersona,
        bool isActive,
        DateTimeOffset createdAt,
        Guid usuarioCreacionId,
        DateTimeOffset? updatedAt)
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
        FechaNacimiento = fechaNacimiento;
        Genero = genero;
        CorreoFacturacionElectronica = correoFacturacionElectronica;
        TipoCliente = tipoCliente;
        ObligadoContabilidad = obligadoContabilidad;
        EsContribuyenteEspecial = esContribuyenteEspecial;
        PermiteCredito = permiteCredito;
        LimiteCredito = limiteCredito;
        DiasCreditoMaximo = diasCreditoMaximo;
        EstadoCredito = estadoCredito;
        RolesPersona = rolesPersona;
        IsActive = isActive;
        CreatedAt = createdAt;
        UsuarioCreacionId = usuarioCreacionId;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public Guid PersonaId { get; }

    public Guid EmpresaId { get; }

    public string TipoIdentificacion { get; }

    public string Identificacion { get; }

    public string RazonSocialONombresCompletos { get; }

    public string? NombreComercial { get; }

    public string DireccionPrincipal { get; }

    public string? CorreoElectronicoPrincipal { get; }

    public string? TelefonoCelular { get; }

    public DateOnly? FechaNacimiento { get; }

    public string? Genero { get; }

    public string? CorreoFacturacionElectronica { get; }

    public string TipoCliente { get; }

    public bool ObligadoContabilidad { get; }

    public bool EsContribuyenteEspecial { get; }

    public bool PermiteCredito { get; }

    public decimal LimiteCredito { get; }

    public int DiasCreditoMaximo { get; }

    public string EstadoCredito { get; }

    public string NombreCompleto => RazonSocialONombresCompletos;

    public IReadOnlyCollection<string> RolesPersona { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public Guid UsuarioCreacionId { get; }

    public DateTimeOffset? UpdatedAt { get; }

    public Guid? UsuarioModificacionId => UpdatedAt.HasValue ? UsuarioCreacionId : null;
}
