namespace TestDeIa.Infrastructure.Persistence.Entities;

public sealed class SecurityUserPuntoEmisionEntity
{
    public Guid SecurityUserId { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid EmpresaPuntoEmisionId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
