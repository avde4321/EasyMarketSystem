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

    public IReadOnlyCollection<string> RolesPersona { get; set; } = [];

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
