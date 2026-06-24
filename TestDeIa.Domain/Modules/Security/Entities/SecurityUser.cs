namespace TestDeIa.Domain.Modules.Security.Entities;

public sealed class SecurityUser
{
    public SecurityUser(
        Guid id,
        Guid empresaId,
        Guid personaId,
        string userName,
        string displayName,
        string email,
        string passwordHash,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<UserEmpresaAcceso> empresasAcceso,
        IReadOnlyCollection<string> rolesPersona,
        bool isActive,
        DateTimeOffset createdAt)
    {
        Id = id;
        EmpresaId = empresaId;
        PersonaId = personaId;
        UserName = userName;
        DisplayName = displayName;
        Email = email;
        PasswordHash = passwordHash;
        Roles = roles;
        EmpresasAcceso = empresasAcceso;
        RolesPersona = rolesPersona;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    public Guid EmpresaId { get; }

    public Guid PersonaId { get; }

    public string UserName { get; }

    public string DisplayName { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    public IReadOnlyCollection<string> Roles { get; }

    public IReadOnlyCollection<UserEmpresaAcceso> EmpresasAcceso { get; }

    public IReadOnlyCollection<string> RolesPersona { get; }

    public bool IsActive { get; }

    public DateTimeOffset CreatedAt { get; }
}
