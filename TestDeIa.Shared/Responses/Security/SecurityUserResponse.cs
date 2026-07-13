namespace TestDeIa.Shared.Responses.Security;

public sealed class SecurityUserResponse
{
    public Guid Id { get; set; }

    public Guid PersonaId { get; set; }

    public string PersonaIdentificacion { get; set; } = string.Empty;

    public string PersonaNombre { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; set; } = [];

    public IReadOnlyCollection<string> Permissions { get; set; } = [];

    public IReadOnlyCollection<string> RolesPersona { get; set; } = [];

    public string RolPrincipal { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool IsBlocked { get; set; }

    public bool IsBlockedManually { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int IntentosFallidos { get; set; }

    public DateTimeOffset? BloqueadoHasta { get; set; }

    public DateTimeOffset? UltimoAcceso { get; set; }
}
