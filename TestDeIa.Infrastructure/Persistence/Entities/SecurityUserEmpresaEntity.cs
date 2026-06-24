namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityUserEmpresaEntity
{
    public Guid SecurityUserId { get; set; }

    public SecurityUserEntity SecurityUser { get; set; } = default!;

    public Guid EmpresaId { get; set; }

    public EmpresaEmisoraEntity Empresa { get; set; } = default!;

    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
