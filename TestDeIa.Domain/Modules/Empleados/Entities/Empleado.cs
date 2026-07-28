namespace TestDeIa.Domain.Modules.Empleados.Entities;

public sealed class Empleado
{
    public Empleado(
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
        string? codigoEmpleado,
        string? codigoBiometrico,
        DateOnly? fechaIngreso,
        DateOnly? fechaSalida,
        string tipoContrato,
        string? cargoPuesto,
        decimal sueldoBase,
        decimal porcentajeComisionVentas,
        string estadoLaboral,
        string? nombreContactoEmergencia,
        string? telefonoEmergencia,
        IReadOnlyCollection<string> rolesPersona,
        bool isActive,
        DateTimeOffset createdAt,
        Guid usuarioCreacionId,
        DateTimeOffset? updatedAt,
        string? regionCodigo = null,
        string? provinciaCodigo = null,
        string? ciudadCodigo = null,
        string? sectorCodigo = null)
    {
        if (id != personaId)
        {
            throw new ArgumentException("El Id del empleado debe ser el mismo Id de la persona asociada.", nameof(id));
        }

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
        CodigoEmpleado = codigoEmpleado;
        CodigoBiometrico = codigoBiometrico;
        FechaIngreso = fechaIngreso;
        FechaSalida = fechaSalida;
        TipoContrato = tipoContrato;
        CargoPuesto = cargoPuesto;
        SueldoBase = sueldoBase;
        PorcentajeComisionVentas = porcentajeComisionVentas;
        EstadoLaboral = estadoLaboral;
        NombreContactoEmergencia = nombreContactoEmergencia;
        TelefonoEmergencia = telefonoEmergencia;
        RolesPersona = rolesPersona;
        RegionCodigo = regionCodigo;
        ProvinciaCodigo = provinciaCodigo;
        CiudadCodigo = ciudadCodigo;
        SectorCodigo = sectorCodigo;
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

    public string? CodigoEmpleado { get; }

    public string? CodigoBiometrico { get; }

    public DateOnly? FechaIngreso { get; }

    public DateOnly? FechaSalida { get; }

    public string TipoContrato { get; }

    public string? CargoPuesto { get; }

    public decimal SueldoBase { get; }

    public decimal PorcentajeComisionVentas { get; }

    public string EstadoLaboral { get; }

    public string? NombreContactoEmergencia { get; }

    public string? TelefonoEmergencia { get; }

    public string NombreCompleto => RazonSocialONombresCompletos;

    public IReadOnlyCollection<string> RolesPersona { get; }

    public string? RegionCodigo { get; }

    public string? ProvinciaCodigo { get; }

    public string? CiudadCodigo { get; }

    public string? SectorCodigo { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }

    public Guid UsuarioCreacionId { get; }

    public DateTimeOffset? UpdatedAt { get; }

    public Guid? UsuarioModificacionId => UpdatedAt.HasValue ? UsuarioCreacionId : null;
}
