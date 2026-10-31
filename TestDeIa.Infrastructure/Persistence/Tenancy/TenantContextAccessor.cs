using TestDeIa.Application.Common;

namespace TestDeIa.Infrastructure.Persistence.Tenancy;

public sealed class TenantContextAccessor : ITenantContextAccessor
{
    public Guid? EmpresaId { get; set; }
    public Guid? UserId { get; set; }
    public bool IsSystemContext { get; set; }
}
