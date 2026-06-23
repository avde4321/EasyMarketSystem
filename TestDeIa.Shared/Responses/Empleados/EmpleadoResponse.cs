namespace TestDeIa.Shared.Responses.Empleados;

public sealed class EmpleadoResponse
{
    public Guid Id { get; set; }

    public Guid PersonaId { get; set; }

    public string TipoIdentificacion { get; set; } = string.Empty;

    public string Identificacion { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public IReadOnlyCollection<string> RolesPersona { get; set; } = [];

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
