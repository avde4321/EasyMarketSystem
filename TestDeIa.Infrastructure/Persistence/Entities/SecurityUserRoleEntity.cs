namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityUserRoleEntity
{
    public Guid UserId { get; set; }

    public SecurityUserEntity User { get; set; } = default!;

    public Guid RoleId { get; set; }

    public SecurityRoleEntity Role { get; set; } = default!;
}
