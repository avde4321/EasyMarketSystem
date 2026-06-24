namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityUserEntity
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }

    public Guid PersonaId { get; set; }

    public PersonaEntity Persona { get; set; } = default!;

    public string UserName { get; set; } = string.Empty;

    public string NormalizedUserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<SecurityUserRoleEntity> UserRoles { get; set; } = [];

    public ICollection<SecurityUserEmpresaEntity> EmpresasAcceso { get; set; } = [];
}
