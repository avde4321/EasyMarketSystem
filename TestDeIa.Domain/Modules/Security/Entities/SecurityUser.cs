namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityUser
{
    public SecurityUser(
        Guid id,
        Guid personaId,
        string userName,
        string displayName,
        string email,
        string passwordHash,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> rolesPersona,
        bool isActive,
        DateTimeOffset createdAt)
    {
        Id = id;
        PersonaId = personaId;
        UserName = userName;
        DisplayName = displayName;
        Email = email;
        PasswordHash = passwordHash;
        Roles = roles;
        RolesPersona = rolesPersona;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public Guid PersonaId { get; }

    public string UserName { get; }

    public string DisplayName { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    public IReadOnlyCollection<string> Roles { get; }

    public IReadOnlyCollection<string> RolesPersona { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }
}
